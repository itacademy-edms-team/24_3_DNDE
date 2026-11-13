using System.Text.Json;
using Common.Application.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sets.Domain.Sets;
using Sets.Infrastructure.Data;
using Sets.Infrastructure.Data.Outbox;
using Tests.Integration.Helpers;

namespace Tests.Integration.Sets;

/// <summary>
/// Outbox на доменных событиях: interceptor материализует события в строки, воркер доставляет их
/// обработчикам и ведёт жизненный цикл pending/processed/retrying/dead (ADR 0040).
/// Таблица общая для прогона, поэтому тест находит своё сообщение по id (jsonb не ищется LIKE).
/// </summary>
public sealed class OutboxTests : IClassFixture<AppFactory>
{
    private readonly AppFactory _factory;

    public OutboxTests(AppFactory factory) => _factory = factory;

    [DatabaseFact]
    public async Task RegisterDomainEvent_OnSave_MaterializesOutboxRow()
    {
        var marker = UniqueMarker();
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SetsDbContext>();

        var set = Set.Create(Guid.NewGuid(), "Набор", null, DateTime.UtcNow).Value;
        set.RegisterDomainEvent(new OutboxTestEvent(marker));
        context.Sets.Add(set);
        await context.SaveChangesAsync();

        var row = await context.OutboxMessages
            .Where(message => message.Type == "tests.outbox.record")
            .OrderByDescending(message => message.Id)
            .FirstAsync();

        Assert.Equal(OutboxStatus.Pending, row.Status);
        Assert.Equal(OutboxLanes.Fast, row.Lane);
        Assert.Equal(1, row.Version);

        using var payload = JsonDocument.Parse(row.Payload);
        Assert.Equal(marker, payload.RootElement.GetProperty("marker").GetString());
    }

    [DatabaseFact]
    public async Task Processor_DeliversMessage_MarksProcessedAndInvokesHandler()
    {
        var marker = UniqueMarker();
        var id = await AddMessageAsync("tests.outbox.record", new OutboxTestEvent(marker));

        await DrainAsync(OutboxLanes.Fast);

        var row = await FindByIdAsync(id);
        Assert.Equal(OutboxStatus.Processed, row.Status);
        Assert.Equal(0, row.Attempts);

        var recorder = Recorder();
        Assert.Contains(recorder.Markers, recorded => recorded == marker);
    }

    [DatabaseFact]
    public async Task Processor_HandlerFails_RetriesWithBackoff()
    {
        var id = await AddMessageAsync("tests.outbox.fail", new OutboxFailingEvent(UniqueMarker()));

        await DrainAsync(OutboxLanes.Fast);

        var row = await FindByIdAsync(id);
        Assert.Equal(OutboxStatus.Retrying, row.Status);
        Assert.Equal(1, row.Attempts);
        Assert.True(row.NextAttemptAt > DateTime.UtcNow);
        Assert.Contains("Test.OutboxFailed", row.LastError);
    }

    [DatabaseFact]
    public async Task Processor_AttemptsExhausted_MarksDead()
    {
        var id = await AddMessageAsync("tests.outbox.fail", new OutboxFailingEvent(UniqueMarker()), attempts: 9);

        await DrainAsync(OutboxLanes.Fast);

        var row = await FindByIdAsync(id);
        Assert.Equal(OutboxStatus.Dead, row.Status);
        Assert.Equal(10, row.Attempts);
    }

    [DatabaseFact]
    public async Task Processor_MessageNotDueYet_IsSkipped()
    {
        var id = await AddMessageAsync("tests.outbox.record", new OutboxTestEvent(UniqueMarker()), nextAttemptAt: DateTime.UtcNow.AddHours(1));

        await DrainAsync(OutboxLanes.Fast);

        var row = await FindByIdAsync(id);
        Assert.Equal(OutboxStatus.Pending, row.Status);
    }

    [DatabaseFact]
    public async Task Processor_OtherLane_IsSkipped()
    {
        var id = await AddMessageAsync("tests.outbox.record", new OutboxTestEvent(UniqueMarker()), lane: OutboxLanes.Slow);

        await DrainAsync(OutboxLanes.Fast);

        var row = await FindByIdAsync(id);
        Assert.Equal(OutboxStatus.Pending, row.Status);
    }

    private RecordingOutboxHandler Recorder()
    {
        using var scope = _factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<RecordingOutboxHandler>();
    }

    /// <summary>Опустошает очередь полосы: таблица общая, одного батча может не хватить до своего сообщения.</summary>
    private async Task DrainAsync(string lane)
    {
        using var scope = _factory.Services.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<OutboxProcessor>();

        for (var pass = 0; pass < 100; pass++)
            if (await processor.ProcessBatchAsync(lane, batchSize: 50, maxAttempts: 10, CancellationToken.None) == 0)
                return;
    }

    private async Task<long> AddMessageAsync(
        string type,
        object payload,
        string lane = OutboxLanes.Fast,
        int attempts = 0,
        DateTime? nextAttemptAt = null)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SetsDbContext>();
        var now = DateTime.UtcNow;

        var message = new OutboxMessage
        {
            Type = type,
            Version = 1,
            Lane = lane,
            Payload = JsonSerializer.Serialize(payload, new JsonSerializerOptions(JsonSerializerDefaults.Web)),
            Status = OutboxStatus.Pending,
            Attempts = attempts,
            NextAttemptAt = nextAttemptAt ?? now,
            CreatedAt = now,
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

    private static string UniqueMarker() => $"marker-{Guid.NewGuid():N}";
}
