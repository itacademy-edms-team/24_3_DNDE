namespace EDMS1.CommandLog.Primitives.Sequences;

/// <summary>
/// Represents a sequence.
/// </summary>
/// <typeparam name="T">Type of elements at sequence.</typeparam>
public interface ISequence<T> : IEnumerable<T>
{
    /// <summary>
    /// Indicates, if sequence is repeated.
    /// </summary>
    bool IsRepeat { get; }

    /// <summary>
    /// Gets next value from sequence.
    /// </summary>
    /// <returns>Instance of <typeparamref name="T"/>.</returns>
    T Next();
}