namespace HttpLogging;

/// <summary>
/// Контейнер источников для одного лог-события (LogEventInfo, HttpContext, Activity).
/// Собирается один раз на событие и передаётся экстракторам.
/// </summary>
internal sealed class ExtractionContext
{
    private readonly Dictionary<Type, object> _sources = new();

    public void Set<T>(T source) where T : notnull => _sources[typeof(T)] = source;

    public T? Get<T>() where T : class => _sources.TryGetValue(typeof(T), out var source) ? (T)source : null;
}
