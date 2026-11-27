using Microsoft.AspNetCore.Http;

namespace HttpLogging.Abstractions;

/// <summary>
/// Определяет контракт фильтрации запросов, подлежащих логированию.
/// </summary>
/// <remarks>
/// Реализация возвращает <see langword="true"/>, если запрос нужно логировать,
/// и <see langword="false"/>, чтобы пропустить его без захвата тел и записи в БД.
/// </remarks>
public interface IRequestFilter
{
    /// <summary>
    /// Определяет, нужно ли логировать указанный запрос.
    /// </summary>
    /// <param name="context">Контекст HTTP-запроса.</param>
    /// <returns>
    /// <see langword="true"/>, если запрос логируется;
    /// иначе <see langword="false"/>.
    /// </returns>
    bool ShouldLog(HttpContext context);
}
