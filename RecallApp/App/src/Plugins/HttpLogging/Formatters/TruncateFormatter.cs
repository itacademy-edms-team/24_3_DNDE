using HttpLogging.Abstractions;

namespace HttpLogging.Formatters;

/// <summary>
/// Представляет форматтер, обрезающий строковое представление данных до указанной длины.
/// </summary>
internal sealed class TruncateFormatter(int maxLength) : ILogFormatter
{
    /// <summary>
    /// Форматирует данные, обрезая их строковое представление до указанной длины.
    /// </summary>
    /// <param name="sanitizedData">Замаскированные данные.</param>
    /// <returns>
    /// Обрезанная строка
    /// или <see langword="null"/>.
    /// </returns>
    public string? Format(object? sanitizedData)
    {
        var text = SanitizedValue.ToDisplayString(sanitizedData);
        if (string.IsNullOrEmpty(text))
            return null;

        return maxLength > 0 && text.Length > maxLength ? text[..maxLength] : text;
    }
}
