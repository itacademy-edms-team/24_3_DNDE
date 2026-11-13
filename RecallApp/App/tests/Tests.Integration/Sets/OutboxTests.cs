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
/// Таблица outbox общая для всего прогона, поэтому каждый тест помечает свои сообщения уникальным
/// идентификатором - в `type` для проверок записи и по `id` для проверок доставки - а не глобальными счётчиками.
/// </summary>
public sealed class OutboxTests : IClassFixture<AppFactory>
{
    private const string NoHandlerPrefix = "tests.outbox.orphan.";

    private readonly AppFactory _factory;

    public OutboxTests(AppFactory factory) => _factory = factory;

    [DatabaseFact]
    public async Task Enqueue_InsideRolledBackTransaction_LeavesNoMessage()
    {
        var type = UniqueType();

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SetsDbContext>();
        var writer = scope.ServiceProvider.GetRequiredService<IOutboxWriter>();

        await using (var transaction = await context.Database.BeginTransactionAsync())
        {
            writer.Enqueue(type, new { marker = "rolled-back" });
            await context.SaveChangesAsync();
            await transaction.RollbackAsync();
        }

        Assert.False(await TypeExistsAsync(type));
    }

    [DatabaseFact]
    public async Task Enqueue_WithBusinessChange_PersistsPendingMessage()
    {
        var type = UniqueType();

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SetsDbContext>();
        var writer = scope.ServiceProvider.GetRequiredService<IOutboxWriter>();

        writer.Enqueue(type, new { marker = "persisted" });
        await context.SaveChangesAsync();

        var message = await FindByTypeAsync(type);
        Assert.Null(message.ProcessedAt);
        Assert.Equal(0, message.Attempts);
    }

    [DatabaseFact]
    public async Task ProcessBatch_RegisteredHandler_MarksMessageDelivered()
    {
        var marker = UniqueMarker("delivered");
        var messageId = await AddMessageAsync(RecordingOutboxHandler.Type, new { marker });

        await ProcessAsync();

        var message = await FindByIdAsync(messageId);
        Assert.NotNull(message.ProcessedAt);
        Assert.Equal(0, message.Attempts);

        var recorder = Recorder();
        Assert.Contains(recorder.Payloads, payload => payload.Contains(marker));
    }

    [DatabaseFact]
    public async Task ProcessBatch_HandlerFails_SchedulesRetryWithBackoff()
    {
        var messageId = await AddMessageAsync(FailingOutboxHandler.Type, new { marker = "failing" });

        await ProcessAsync();

        var message = await FindByIdAsync(messageId);
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

        var message = await FindByIdAsync(messageId);
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

        await ProcessAsync();

        var message = await FindByIdAsync(messageId);
        Assert.Null(message.ProcessedAt);
        Assert.Equal(0, message.Attempts);
    }

    private RecordingOutboxHandler Recorder()
    {
        using var scope = _factory.Services.CreateScope();
        return scope.ServiceProvider.GetServices<IOutboxMessageHandler>()
            .OfType<RecordingOutboxHandler>()
            .Single();
    }

    /// <summary>
    /// Опустошает очередь: в общем прогоне таблица накапливает сообщения других тестов,
    /// поэтому одного батча может не хватить, чтобы дойти до сообщения этого теста.
    /// Цикл идёт, пока есть готовые к доставке; сообщения без обработчика и отказавшие
    /// уходят в отложенную попытку и больше не попадают в выборку текущего прохода.
    /// </summary>
    private async Task ProcessAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<OutboxProcessor>();

        for (var pass = 0; pass < 100; pass++)
        {
            if (await processor.ProcessBatchAsync(batchSize: 50, CancellationToken.None) == 0)
                return;
        }
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

    private async Task<OutboxMessage> FindByIdAsync(long id)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SetsDbContext>();
        return await context.OutboxMessages.SingleAsync(row => row.Id == id);
    }

    private async Task<OutboxMessage> FindByTypeAsync(string type)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SetsDbContext>();
        return await context.OutboxMessages.SingleAsync(row => row.Type == type);
    }

    private async Task<bool> TypeExistsAsync(string type)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SetsDbContext>();
        return await context.OutboxMessages.AnyAsync(row => row.Type == type);
    }

    /// <summary>Тип без зарегистрированного обработчика: сообщение никуда не доставится, только проверяется запись.</summary>
    private static string UniqueType() => NoHandlerPrefix + Guid.NewGuid().ToString("N");

    private static string UniqueMarker(string suffix) => $"outbox-{suffix}-{Guid.NewGuid():N}";
}
