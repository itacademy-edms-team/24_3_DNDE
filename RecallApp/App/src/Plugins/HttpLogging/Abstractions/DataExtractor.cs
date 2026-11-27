namespace HttpLogging.Abstractions;

/// <summary>
/// Предоставляет базовый класс для экстракторов,
/// извлекающих данные из источника типа <typeparamref name="TSource"/>.
/// </summary>
/// <typeparam name="TSource">Тип источника, из которого извлекаются данные.</typeparam>
/// <remarks>
/// Базовый класс достаёт источник из <see cref="ExtractionContext"/> и делегирует
/// конкретное извлечение методу <see cref="ExtractFrom"/> производного класса.
/// </remarks>
internal abstract class DataExtractor<TSource> : IDataExtractor where TSource : class
{
    /// <summary>
    /// Извлекает данные из указанного контекста.
    /// </summary>
    /// <param name="context">Контекст, содержащий источники лог-события.</param>
    /// <returns>
    /// Извлечённые данные
    /// или <see langword="null"/>, если требуемый источник отсутствует.
    /// </returns>
    public object? Extract(ExtractionContext context)
    {
        var source = context.Get<TSource>();
        return source is null ? null : ExtractFrom(source);
    }

    /// <summary>
    /// Извлекает данные из источника.
    /// </summary>
    /// <param name="source">Источник данных.</param>
    /// <returns>
    /// Извлечённые данные
    /// или <see langword="null"/>.
    /// </returns>
    protected abstract object? ExtractFrom(TSource source);
}
