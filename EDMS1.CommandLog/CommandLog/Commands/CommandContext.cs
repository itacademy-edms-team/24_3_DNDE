namespace EDMS1.CommandLog.Commands;

/// <summary>
/// Реализация <see cref="ICommandContext"/>.
/// </summary>
public class CommandContext : ICommandContext
{
    /// <inheritdoc/>
    public ICommand? MainCommand { get; private set; }

    /// <inheritdoc/>
    public Guid TransactionId { get; private set; }

    /// <summary>
    /// Задать команду.
    /// </summary>
    public void SetCommandIfNotNull(ICommand command)
    {
        MainCommand ??= command;
    }

    /// <summary>
    /// Задать команду.
    /// </summary>
    public void SetTransactionId(Guid transactionId)
    {
        TransactionId = transactionId;
    }
}
