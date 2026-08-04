namespace EDMS1.CommandLog.Commands;

/// <summary>
/// Конекст команды.
/// Позволяет получить доступ к данным команды в изолированных сервисах и обработчиках.
/// Если выполняется команда в команде, то в контексте будет выставлена самая первая команда.
/// </summary>
public interface ICommandContext
{
    /// <summary>
    /// Главная команда, внутренние команды (команда в команде) сюда не записываются.
    /// </summary>
    public ICommand? MainCommand { get; }

    /// <summary>
    /// Идентификатор транзакции.
    /// </summary>
    public Guid TransactionId { get; }

    /// <summary>
    /// Токен корреляции для сквозных процессов.
    /// </summary>
    public Guid? CorrelationToken => MainCommand?.CorrelationToken;

    /// <summary>
    /// Проверка, что сейчас выполняется команда.
    /// </summary>
    public bool IsCommandExecuting => MainCommand is not null;
}
