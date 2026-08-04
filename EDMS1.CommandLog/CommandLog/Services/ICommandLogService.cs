using EDMS1.CommandLog.Commands;
using EDMS1.CommandLog.Models;

namespace EDMS1.CommandLog.Services;

/// <summary>
/// Сервис логирования вызовов команд.
/// </summary>
public interface ICommandLogService
{
    /// <summary>
    /// Логирование успешной команды.
    /// </summary>
    Task LogCommandAsync(ICommand command, CommandResult result, Guid transactionId);

    /// <summary>
    /// Логирование неудачной команды.
    /// </summary>
    Task LogFailedCommandAsync(ICommand command, Guid transactionId, string message);

    /// <summary>
    /// Установка комманды на повторную обработку.
    /// </summary>
    Task LogRetryCommandAsync(Guid id);

    /// <summary>
    /// Выполнить повторную обработку комманды.
    /// </summary>
    Task RetryCommandNowAsync(Guid id, CancellationToken cancel);

    /// <summary>
    /// Запуск процесса повторной обработки команд со статусом Retry.
    /// </summary>
    Task RetryCommandsAsync(CancellationToken cancel);
}
