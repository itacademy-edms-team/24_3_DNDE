using System.Text.Json;
using Common.Application.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sets.Application.Abstractions;
using Sets.Infrastructure.Data;
using Sets.Infrastructure.Data.Outbox;
using Tests.Integration.Helpers;

namespace Tests.Integration.Sets;

/// <summary>
/// Transactional outbox модуля Sets (ADR 0040): запись в одной транзакции с бизнес-изменением,
/// доставка зарегистрированным обработчикам, повторы с паузой и разбор «отравленных» сообщений.
/// </summary>
public sealed class OutboxTests : IClassFixture<AppFactory>
{
    private readonly AppFactory _factory;

    public OutboxTests(AppFactory factory) => _factory = factory;

    [DatabaseFact]
    public async Task Enqueue_InsideRolledBackTransaction_LeavesNoMessage()
    {
        var before = await CountMessagesAsync();

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SetsDbContext>();
        var writer = scope.ServiceProvider.GetRequiredService<IOutboxWriter>();

        await using (var transaction = await context.Database.BeginTransactionAsync())
        {
            writer.Enqueue(RecordingOutboxHandler.Type, new { marker = "rolled-back" });
            await context.SaveChangesAsync();
            await transaction.RollbackAsync();
        }

        Assert.Equal(before, await CountMessagesAsync());
    }

    [DatabaseFact]
    public async Task Enqueue_WithBusinessChange_PersistsPendingMessage()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SetsDbContext>();
        var writer = scope.ServiceProvider.GetRequiredService<IOutboxWriter>();

        var before = await CountMessagesAsync();
        writer.Enqueue(RecordingOutboxHandler.Type, new { marker = "persisted" });
        await context.SaveChangesAsync();

        var added = await LatestMessagesAsync(before);
        var message = Assert.Single(added);
        Assert.Null(message.ProcessedAt);
        Assert.Equal(0, message.Attempts);
    }

    [DatabaseFact]
    public async Task ProcessBatch_RegisteredHandler_MarksMessageDelivered()
    {
        var messageId = await AddMessageAsync(RecordingOutboxHandler.Type, new { marker = "delivered" });

        var delivered = await ProcessAsync();

        Assert.Equal(1, delivered);
        var message = await FindMessageAsync(messageId);
        Assert.NotNull(message.ProcessedAt);
        Assert.Equal(0, message.Attempts);

        using var recorderScope = _factory.Services.CreateScope();
        var recorder = recorderScope.ServiceProvider.GetServices<IOutboxMessageHandler>()
            .OfType<RecordingOutboxHandler>()
            .Single();
        Assert.Contains(recorder.Payloads, payload => payload.Contains("delivered"));
    }

    [DatabaseFact]
    public async Task ProcessBatch_HandlerFails_SchedulesRetryWithBackoff()
    {
        var messageId = await AddMessageAsync(FailingOutboxHandler.Type, new { marker = "failing" });

        var delivered = await ProcessAsync();

        Assert.Equal(0, delivered);
        var message = await FindMessageAsync(messageId);
        Assert.Null(message.ProcessedAt);
        Assert.Null(message.AbandonedAt);
        Assert.Equal(1, message.Attempts);
        Assert.True(message.NextAttemptAt > DateTime.UtcNow);
        Assert.Contains("Test.OutboxFailed", message.LastError);
    }

    [DatabaseFact]
    public async Task ProcessBatch_AttemptsExhausted_MarksMessageAbandoned()
    {
        var messageId = await AddMessageAsync(FailingOutboxHandler.Type, new { marker = "abandoned" }, attempts: 9);

        await ProcessAsync();

        var message = await FindMessageAsync(messageId);
        Assert.NotNull(message.AbandonedAt);
        Assert.Equal(10, message.Attempts);
    }

    [DatabaseFact]
    public async Task ProcessBatch_MessageNotDueYet_IsNotDelivered()
    {
        var messageId = await AddMessageAsync(
            RecordingOutboxHandler.Type,
            new { marker = "future" },
            nextAttemptAt: DateTime.UtcNow.AddHours(1));

        var delivered = await ProcessAsync();

        Assert.Equal(0, delivered);
        var message = await FindMessageAsync(messageId);
        Assert.Null(message.ProcessedAt);
    }

    private async Task<int> ProcessAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<OutboxProcessor>();
        return await processor.ProcessBatchAsync(batchSize: 50, CancellationToken.None);
    }

    private async Task<long> AddMessageAsync(
        string type,
        object payload,
        int attempts = 0,
        DateTime? nextAttemptAt = null)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SetsDbContext>();

        var message = new OutboxMessage
        {
            Type = type,
            Payload = JsonSerializer.Serialize(payload, new JsonSerializerOptions(JsonSerializerDefaults.Web)),
            CreatedAt = DateTime.UtcNow,
            NextAttemptAt = nextAttemptAt ?? DateTime.UtcNow,
            Attempts = attempts,
        };

        context.OutboxMessages.Add(message);
        await context.SaveChangesAsync();
        return message.Id;
    }

    private async Task<OutboxMessage> FindMessageAsync(long messageId)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SetsDbContext>();
        return await context.OutboxMessages.SingleAsync(row => row.Id == messageId);
    }

    private async Task<OutboxMessage[]> LatestMessagesAsync(int skipCount)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SetsDbContext>();
        return await context.OutboxMessages
            .OrderBy(row => row.Id)
            .Skip(skipCount)
            .ToArrayAsync();
    }

    private async Task<int> CountMessagesAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SetsDbContext>();
        return await context.OutboxMessages.CountAsync();
    }
}
