using HttpLogging.Abstractions;
using Microsoft.AspNetCore.Http;

namespace HttpLogging;

/// <summary>
/// Фильтр по умолчанию: логирует все запросы.
/// </summary>
internal sealed class LogAllRequestFilter : IRequestFilter
{
    public bool ShouldLog(HttpContext context) => true;
}
