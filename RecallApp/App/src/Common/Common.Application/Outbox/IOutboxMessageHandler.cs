using System.Text.Json;
using ErrorOr;

namespace Common.Application.Outbox;

/// <summary>
/// Обработчик одного типа сообщений outbox. Регистрируется модулем-потребителем;
/// продюсер о нём не знает - он пишет только строку типа и payload (ADR 0040).
/// </summary>
public interface IOutboxMessageHandler
{
    /// <summary>Строка типа сообщения, по которой диспетчер выбирает обработчик.</summary>
    string MessageType { get; }

    /// <summary>
    /// Применяет сообщение.
    /// Повторный вызов с тем же payload обязан быть безопасным: доставка at-least-once.
    /// </summary>
    Task<ErrorOr<Success>> HandleAsync(JsonElement payload, CancellationToken cancellationToken);
}
