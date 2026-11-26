using HttpLogging.Abstractions;
using Microsoft.AspNetCore.Http;

namespace HttpLogging.RequestFilters;

/// <summary>
/// Комбинирует несколько фильтров:
/// запрос логируется, только если все вложенные фильтры разрешают логирование.
/// </summary>
public sealed class CompositeRequestFilter : IRequestFilter
{
    private readonly IRequestFilter[] _filters;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="CompositeRequestFilter"/>.
    /// </summary>
    /// <param name="filters">Вложенные фильтры.</param>
    public CompositeRequestFilter(params IRequestFilter[] filters) => _filters = filters;

    /// <summary>
    /// Определяет, нужно ли логировать запрос.
    /// </summary>
    /// <param name="context">Контекст HTTP-запроса.</param>
    /// <returns>
    /// <see langword="true"/>, если все вложенные фильтры разрешают логирование;
    /// иначе <see langword="false"/>
    /// </returns>
    public bool ShouldLog(HttpContext context)
    {
        foreach (var filter in _filters)
        {
            if (!filter.ShouldLog(context))
                return false;
        }

        return true;
    }
}
