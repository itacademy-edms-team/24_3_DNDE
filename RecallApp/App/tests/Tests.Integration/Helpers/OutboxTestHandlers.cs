using System.Collections.Concurrent;
using System.Text.Json;
using Common.Application.Outbox;
using ErrorOr;

namespace Tests.Integration.Helpers;

/// <summary>
/// Тестовый обработчик outbox: записывает payload успешно доставленных сообщений,
/// чтобы тест мог проверить, что диспетчер действительно вызвал обработчик.
/// </summary>
public sealed class RecordingOutboxHandler : IOutboxMessageHandler
{
    public const string Type = "tests.outbox.record";

    private readonly ConcurrentQueue<string> _payloads = new();

    public string MessageType => Type;

    public IReadOnlyCollection<string> Payloads => _payloads.ToArray();

    public Task<ErrorOr<Success>> HandleAsync(JsonElement payload, CancellationToken cancellationToken)
    {
        _payloads.Enqueue(payload.GetRawText());
        return Task.FromResult<ErrorOr<Success>>(Result.Success);
    }
}

/// <summary>Тестовый обработчик, который всегда отказывает: проверяет повторы и накопление ошибки.</summary>
public sealed class FailingOutboxHandler : IOutboxMessageHandler
{
    public const string Type = "tests.outbox.fail";

    public string MessageType => Type;

    public Task<ErrorOr<Success>> HandleAsync(JsonElement payload, CancellationToken cancellationToken) =>
        Task.FromResult<ErrorOr<Success>>(Error.Failure("Test.OutboxFailed", "Обработчик намеренно отказывает."));
}
