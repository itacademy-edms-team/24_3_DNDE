namespace HttpLogging.Abstractions;

/// <summary>
/// Определяет контракт извлечения исходных данных из контекста лог-события.
/// </summary>
/// <remarks>
/// Экстрактор только достаёт данные из <see cref="ExtractionContext"/>.
/// Дальнейшая обработка выполняется рендерерами, <see cref="ISanitizer"/> и <see cref="ILogFormatter"/>.
/// </remarks>
internal interface IDataExtractor
{
    /// <summary>
    /// Извлекает данные из указанного контекста.
    /// </summary>
    /// <param name="context">Контекст, содержащий источники лог-события
    /// (LogEventInfo, HttpContext, Activity, ...).</param>
    /// <returns>
    /// Извлечённые данные
    /// или <see langword="null"/>, если требуемый источник в контексте отсутствует.
    /// </returns>
    object? Extract(ExtractionContext context);
}
