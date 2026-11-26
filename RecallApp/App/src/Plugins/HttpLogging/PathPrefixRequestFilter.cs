using HttpLogging.Abstractions;
using Microsoft.AspNetCore.Http;

namespace HttpLogging;

/// <summary>
/// Фильтрует запросы по префиксам пути:
/// запросы, путь которых начинается с одного из указанных префиксов (по границе сегмента), не логируются.
/// </summary>
public sealed class PathPrefixRequestFilter : IRequestFilter
{
    private readonly PathString[] _prefixes;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="PathPrefixRequestFilter"/>.
    /// </summary>
    /// <param name="prefixes">Префиксы путей, которые не логируются (например, "/health").</param>
    public PathPrefixRequestFilter(params string[] prefixes)
        => _prefixes = Array.ConvertAll(prefixes, static prefix => new PathString(prefix));

    /// <summary>
    /// Определяет, нужно ли логировать запрос.
    /// </summary>
    /// <param name="context">Контекст HTTP-запроса.</param>
    /// <returns>
    /// <see langword="false"/>, если путь начинается с одного из префиксов;
    /// иначе <see langword="true"/>.
    /// </returns>
    public bool ShouldLog(HttpContext context)
    {
        foreach (var prefix in _prefixes)
        {
            if (context.Request.Path.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }
}
