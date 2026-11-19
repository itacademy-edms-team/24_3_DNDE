namespace Media.Domain.Objects;

/// <summary>
/// Предоставляет белый список типов изображений, которые принимает модуль.
/// </summary>
/// <remarks>
/// <para>На инициализации загрузки проверяется заявленный клиентом тип: он определяет политику
/// загрузки в хранилище.</para>
/// <para>При подтверждении загрузки заявленный тип уже не решает: формат определяется по сигнатуре файла,
/// поэтому подмена содержимого под чужой <c>Content-Type</c> не проходит (онтология Media §6.3).</para>
/// </remarks>
public static class MediaContentTypes
{
    /// <summary>Тип PNG.</summary>
    public const string Png = "image/png";

    /// <summary>Тип JPEG.</summary>
    public const string Jpeg = "image/jpeg";

    /// <summary>Тип WebP.</summary>
    public const string Webp = "image/webp";

    private static readonly HashSet<string> Supported = new(StringComparer.OrdinalIgnoreCase)
    {
        Png,
        Jpeg,
        Webp,
    };

    /// <summary>
    /// Приводит тип к канонической форме и проверяет, что он в белом списке.
    /// </summary>
    /// <param name="value">Тип из запроса; проходит нормализацию.</param>
    /// <param name="normalized">Приведённый тип в нижнем регистре;
    /// заполняется всегда - при неподдерживаемом типе содержит значение без изменений или пустую строку.</param>
    /// <returns>
    /// <see langword="true"/>, если тип поддерживается;
    /// иначе <see langword="false"/>.
    /// </returns>
    public static bool TryNormalize(string? value, out string normalized)
    {
        normalized = value?.Trim().ToLowerInvariant() ?? string.Empty;
        return Supported.Contains(normalized);
    }
}
