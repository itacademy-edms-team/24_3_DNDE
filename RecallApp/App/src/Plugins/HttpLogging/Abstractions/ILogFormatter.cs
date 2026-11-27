namespace HttpLogging.Abstractions;

/// <summary>
/// Определяет контракт форматирования замаскированных данных перед записью в лог.
/// </summary>
/// <remarks>
/// Форматтер принимает результат <see cref="ISanitizer"/>
/// и приводит его к строковому представлению, готовому для записи.
/// </remarks>
internal interface ILogFormatter
{
    /// <summary>
    /// Форматирует замаскированные данные в строку.
    /// </summary>
    /// <param name="sanitizedData">Замаскированные данные.</param>
    /// <returns>
    /// Отформатированная строка
    /// или <see langword="null"/>.
    /// </returns>
    string? Format(object? sanitizedData);
}
