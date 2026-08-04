namespace EDMS1.CommandLog.Models;

/// <summary>
/// Результат команды.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CommandResult"/> class.
/// </remarks>
/// <param name="status">Instance of <see cref="CommandStatus"/>.</param>
/// <param name="comment">Comment to result.</param>
public class CommandResult(CommandStatus status, string? comment)
{
    /// <summary>
    /// Успешно.
    /// </summary>
    public static readonly CommandResult Successful = new(CommandStatus.Successful, null);

    /// <summary>
    /// Статус.
    /// </summary>
    public CommandStatus Status { get; } = status;

    /// <summary>
    /// Комментарий.
    /// </summary>
    public string? Comment { get; } = comment;

    /// <summary>
    /// Создать результат со статусом <see cref="CommandStatus.CompletedWithVariation"/>.
    /// </summary>
    /// <param name="comment">Comment to <see cref="CommandResult"/> with <see cref="CommandStatus.CompletedWithVariation"/>.</param>
    /// <exception cref="ArgumentException"><paramref name="comment"/> is empty of whitespace.</exception>
    public static CommandResult CreateCompletedWithVariation(string comment)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(comment, nameof(comment));

        return new CommandResult(CommandStatus.CompletedWithVariation, comment);
    }
}