using System.ComponentModel.DataAnnotations;

namespace Host.Configuration;

/// <summary>
/// Настройки SPA (секция "Spa"). SPA живёт на отдельном origin'е (ADR 0030),
/// поэтому все редиректы «наружу» из BFF идут только на <see cref="BaseUrl"/>.
/// </summary>
public sealed class SpaOptions
{
    public const string SectionName = "Spa";

    /// <summary>Origin SPA без пути и завершающего слэша: к нему приклеивается локальный путь возврата.</summary>
    [Required]
    public string BaseUrl { get; set; } = string.Empty;
}
