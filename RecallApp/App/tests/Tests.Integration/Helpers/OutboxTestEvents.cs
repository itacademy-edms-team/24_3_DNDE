using System.Collections.Concurrent;
using Common.Application.DomainEvents;
using Common.Domain.DomainEvents;
using ErrorOr;

namespace Tests.Integration.Helpers;

/// <summary>Тестовое событие успешной доставки.</summary>
public sealed record OutboxTestEvent(string Marker) : DomainEventBase
{
    public override string EventType => "tests.outbox.record";
}

/// <summary>Тестовое событие, которое обработчик всегда отклоняет.</summary>
public sealed record OutboxFailingEvent(string Marker) : DomainEventBase
{
    public override string EventType => "tests.outbox.fail";
}

/// <summary>Записывает маркеры доставленных событий - проверка, что воркер реально вызвал обработчик.</summary>
public sealed class RecordingOutboxHandler : IDomainEventHandler<OutboxTestEvent>
{
    private readonly ConcurrentQueue<string> _markers = new();

    public IReadOnlyCollection<string> Markers => _markers.ToArray();

    public Task<ErrorOr<Success>> HandleAsync(OutboxTestEvent domainEvent, CancellationToken cancellationToken)
    {
        _markers.Enqueue(domainEvent.Marker);
        return Task.FromResult<ErrorOr<Success>>(Result.Success);
    }
}

/// <summary>Всегда отказывает - проверка ретраев и перехода в dead.</summary>
public sealed class FailingOutboxHandler : IDomainEventHandler<OutboxFailingEvent>
{
    public Task<ErrorOr<Success>> HandleAsync(OutboxFailingEvent domainEvent, CancellationToken cancellationToken) =>
        Task.FromResult<ErrorOr<Success>>(Error.Failure("Test.OutboxFailed", "Обработчик намеренно отказывает."));
}
