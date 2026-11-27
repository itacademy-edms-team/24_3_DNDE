using System.Diagnostics;
using HttpLogging.Abstractions;
using HttpLogging.Extractors;
using Microsoft.AspNetCore.Http;
using NLog;

namespace HttpLogging;

/// <summary>
/// Предоставляет конвейер извлечения и маскирования данных лог-события.
/// </summary>
/// <remarks>
/// Хранит реестр экстракторов и санитайзер. Результат маскирования кэшируется в пределах
/// одного <see cref="LogEventInfo"/>, чтобы обрезанная и сжатая колонки одного источника
/// не обрабатывались дважды. Регистрируется в DI и в NLog ServiceRepository.
/// </remarks>
internal sealed class HttpLoggingPipeline
{
    /// <summary>Префикс служебных ключей плагина в свойствах лог-события.</summary>
    public const string CachePrefix = "__http_logging_";
    private const string ContextKey = CachePrefix + "context";
    private const string SanitizedKeyPrefix = CachePrefix + "sanitized_";

    // Экстракторы не имеют состояния, поэтому общий immutable-реестр безопасен.
    private static readonly Dictionary<string, IDataExtractor> Extractors = new(StringComparer.OrdinalIgnoreCase)
    {
        ["StackTrace"] = new StackTraceExtractor(),
        ["RequestQuery"] = new RequestQueryExtractor(),
        ["RequestHeaders"] = new RequestHeadersExtractor(),
        ["ResponseHeaders"] = new ResponseHeadersExtractor(),
        ["RequestBody"] = new RequestBodyExtractor(),
        ["ResponseBody"] = new ResponseBodyExtractor(),
        ["StatusCode"] = new StatusCodeExtractor(),
        ["DurationMs"] = new DurationMsExtractor(),
        ["StartTime"] = new StartTimeExtractor(),
    };

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ISanitizer _sanitizer;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="HttpLoggingPipeline"/>.
    /// </summary>
    /// <param name="sanitizer">Санитайзер, маскирующий сенситивные данные.</param>
    /// <param name="httpContextAccessor">Аксессор текущего HTTP-контекста.</param>
    public HttpLoggingPipeline(ISanitizer sanitizer, IHttpContextAccessor httpContextAccessor)
    {
        _sanitizer = sanitizer;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>Извлекает и маскирует данные указанного источника.</summary>
    /// <param name="logEvent">Лог-событие, для которого извлекаются данные.</param>
    /// <param name="source">Имя источника данных (например, RequestHeaders, RequestBody).</param>
    /// <returns>Маскированные данные или <see langword="null"/>, если источник не найден.</returns>
    public object? GetSanitized(LogEventInfo logEvent, string source)
    {
        var cacheKey = SanitizedKeyPrefix + source;
        if (logEvent.Properties.TryGetValue(cacheKey, out var cached))
            return cached;

        if (!Extractors.TryGetValue(source, out var extractor))
            return null;

        var context = GetOrCreateContext(logEvent);
        var raw = extractor.Extract(context);
        var sanitized = _sanitizer.Sanitize(raw);
        if (sanitized is not null)
            logEvent.Properties[cacheKey] = sanitized;

        return sanitized;
    }

    private ExtractionContext GetOrCreateContext(LogEventInfo logEvent)
    {
        if (logEvent.Properties.TryGetValue(ContextKey, out var cached) && cached is ExtractionContext context)
            return context;

        var newContext = new ExtractionContext();
        newContext.Set(logEvent);

        var http = _httpContextAccessor.HttpContext;
        if (http is not null)
            newContext.Set(http);

        if (Activity.Current is not null)
            newContext.Set(Activity.Current);

        logEvent.Properties[ContextKey] = newContext;
        return newContext;
    }
}
