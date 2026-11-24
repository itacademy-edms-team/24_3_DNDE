namespace Media.Domain.Objects;

/// <summary>
/// Представляет результат определения формата изображения.
/// </summary>
/// <param name="Format">Распознанный формат; <see langword="null"/>, если сигнатура не совпала
/// ни с одним поддерживаемым форматом.</param>
/// <param name="IsAnimated">Признак анимации: APNG или анимированный WebP.</param>
public readonly record struct FormatProbe(MediaFormat? Format, bool IsAnimated);

/// <summary>
/// Определяет формат изображения по содержимому файла.
/// </summary>
/// <remarks>
/// <para>Формат распознаётся по сигнатуре (magic bytes).
/// Анимация определяется здесь же (ADR 0018).</para>
/// <para>Файл не декодируется: достаточно начала содержимого.
/// Для WebP нужно не меньше 30 байт, остальные сигнатуры короче.</para>
/// </remarks>
public static class MediaFormatDetector
{
    private static ReadOnlySpan<byte> PngSignature => [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    private static ReadOnlySpan<byte> JpegSignature => [0xFF, 0xD8, 0xFF];

    /// <summary>
    /// Определяет формат и признак анимации по первым байтам файла.
    /// </summary>
    /// <param name="content">Содержимое файла или его начало.</param>
    /// <returns>
    /// Распознанный формат и признак анимации;
    /// формат <see langword="null"/>, если сигнатура не совпала ни с одним поддерживаемым форматом.
    /// </returns>
    public static FormatProbe Detect(ReadOnlySpan<byte> content)
    {
        if (content.Length >= PngSignature.Length && content[..PngSignature.Length].SequenceEqual(PngSignature))
            return new FormatProbe(MediaFormat.Png, IsAnimatedPng(content));

        if (content.Length >= JpegSignature.Length && content[..JpegSignature.Length].SequenceEqual(JpegSignature))
            return new FormatProbe(MediaFormat.Jpeg, IsAnimated: false);

        if (TryGetWebpFlags(content, out var webpFlags))
            return new FormatProbe(MediaFormat.Webp, (webpFlags & 0x02) != 0);

        return new FormatProbe(Format: null, IsAnimated: false);
    }

    /// <summary>Проверяет PNG на наличие анимации. APNG определяется наличием чанка `acTL`.</summary>
    private static bool IsAnimatedPng(ReadOnlySpan<byte> content) =>
        content.IndexOf("acTL"u8) >= 0;

    /// <summary>WebP с VP8X-заголовком содержит флаг анимации; статические варианты - `VP8 ` или `VP8L`.</summary>
    private static bool TryGetWebpFlags(ReadOnlySpan<byte> content, out byte flags)
    {
        flags = 0;

        if (content.Length < 30)
            return false;

        if (!content[..4].SequenceEqual("RIFF"u8) || !content[8..12].SequenceEqual("WEBP"u8))
            return false;

        if (!content[12..16].SequenceEqual("VP8X"u8))
            return true; // статический WebP: ни VP8X, ни анимации

        flags = content[20];
        return true;
    }
}
