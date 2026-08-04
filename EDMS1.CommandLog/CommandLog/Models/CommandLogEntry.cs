using EDMS1.CommandLog.Commands;
using EDMS1.CommandLog.Primitives.Sequences;
using Newtonsoft.Json;

namespace EDMS1.CommandLog.Models;

/// <summary>
/// Запись лога команды.
/// </summary>
public class CommandLogEntry
{
    /// <summary>
    /// Идентификатор лога команды.
    /// </summary>
    public Guid CommandLogId { get; set; }

    /// <summary>
    /// Дата и время лога команды.
    /// </summary>
    public DateTime CreationTime { get; set; }

    /// <summary>
    /// Токен корреляции.
    /// </summary>
    public Guid CorrelationToken { get; set; }

    /// <summary>
    /// Идентификатор транзакции.
    /// </summary>
    public Guid TransactionId { get; set; }

    /// <summary>
    /// Имя команды.
    /// </summary>
    public string CommandName { get; set; } = null!;

    /// <summary>
    /// Данные команды.
    /// </summary>
    public string Command { get; set; } = null!;

    /// <summary>
    /// Результат выполнения команды.
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// Сообщение об ошибке.
    /// </summary>
    public string? FailureMessage { get; set; }

    /// <summary>
    /// Комментарий.
    /// </summary>
    public string? Comment { get; set; }

    private static readonly SequentialGuidSequence _guids = new (DateTimeOffset.UtcNow.Ticks);

    /// <summary>
    /// Фабричный метод для создания записи лога команды.
    /// </summary>
    public static CommandLogEntry Create(
        ICommand command,
        Guid transactionId,
        CommandStatus status,
        string? failureMessage,
        string? comment)
    {
        return new CommandLogEntry
        {
            CommandLogId = _guids.Next(),
            CreationTime = DateTime.UtcNow,
            CorrelationToken = command.CorrelationToken,
            TransactionId = transactionId,
            CommandName = command.GetType().Name,
            Command = JsonConvert.SerializeObject(command),
            Status = status.ToString(),
            FailureMessage = failureMessage,
            Comment = comment
        };
    }
}