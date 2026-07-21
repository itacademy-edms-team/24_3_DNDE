namespace EDMS1.CommandLog.Exceptions;

/// <summary>
/// Ошибка не найдено.
/// </summary>
[Serializable]
public class NotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class.
    /// </summary>
    public NotFoundException()
        : this(ErrorMessages.NotFound)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class.
    /// </summary>
    public NotFoundException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class.
    /// </summary>
    public NotFoundException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
