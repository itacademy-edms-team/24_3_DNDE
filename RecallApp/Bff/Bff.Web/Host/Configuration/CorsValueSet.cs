namespace Host.Configuration;

/// <summary>
/// Набор значений для CORS-политики.
/// </summary>
public sealed class CorsValueSet
{
    /// <summary>
    /// Разрешить любые значения.
    /// Несовместимо с непустым <see cref="Values"/> и (для origin'ов) с AllowCredentials.
    /// </summary>
    public bool Any { get; set; }

    /// <summary>
    /// Явный список значений. Для origin'ов допускается wildcard-поддомен вида "https://*.example.local".
    /// </summary>
    public string[] Values { get; set; } = [];
}
