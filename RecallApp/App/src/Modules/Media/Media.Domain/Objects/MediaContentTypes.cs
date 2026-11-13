namespace Media.Domain.Objects;

/// <summary>
/// Белый список типов, которые принимает модуль.
/// Проверяется на `init` (что заявляет клиент)
/// и косвенно на `confirm` - там решает сигнатура файла (онтология Media §6.3).
/// </summary>
public static class MediaContentTypes
{
    public const string Png = "image/png";
    public const string Jpeg = "image/jpeg";
    public const string Webp = "image/webp";

    private static readonly HashSet<string> Supported = new(StringComparer.OrdinalIgnoreCase)
    {
        Png,
        Jpeg,
        Webp,
    };

    /// <summary>Приводит тип к канонической форме и проверяет, что он в белом списке.</summary>
    public static bool TryNormalize(string? value, out string normalized)
    {
        normalized = value?.Trim().ToLowerInvariant() ?? string.Empty;
        return Supported.Contains(normalized);
    }
}
