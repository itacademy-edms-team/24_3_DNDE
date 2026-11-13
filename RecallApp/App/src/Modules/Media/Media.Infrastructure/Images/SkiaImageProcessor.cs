using ErrorOr;
using Media.Application.Images;
using Media.Domain.Objects;
using SkiaSharp;

namespace Media.Infrastructure.Images;

/// <summary>
/// Обработчик изображений на SkiaSharp.
/// Метаданные (EXIF/ICC) не переносятся: результат - новое изображение из пиксельной сетки.
/// </summary>
internal sealed class SkiaImageProcessor : IImageProcessor
{
    /// <summary>Качество изображения.</summary>
    private const int Quality = 82;

    /// <summary>Выполняет декодирование и кодирование.</summary>
    public Task<ErrorOr<ProcessedImage>> ProcessAsync(byte[] source, CancellationToken cancellationToken) =>
        // Task.Run, чтобы не забивать поток обработки запроса.
        Task.Run(() => Process(source), cancellationToken); 

    private static ErrorOr<ProcessedImage> Process(byte[] source)
    {
        var probe = MediaFormatDetector.Detect(source);
        if (probe.Format is null)
            return MediaErrors.InvalidFormat;

        if (probe.IsAnimated)
            return MediaErrors.UnsupportedAnimation;

        using var data = SKData.CreateCopy(source);
        using var codec = SKCodec.Create(data);
        if (codec is null)
            return MediaErrors.ImageCorrupt;

        // Размеры известны из заголовка: лимиты проверяем до полного декодирования.
        var info = codec.Info;
        if (info.Width <= 0 || info.Height <= 0)
            return MediaErrors.ImageCorrupt;

        if (Math.Max(info.Width, info.Height) > MediaConstraints.MaxSide
            || (long)info.Width * info.Height > MediaConstraints.MaxPixels)
            return MediaErrors.ResolutionTooLarge;

        using var bitmap = SKBitmap.Decode(codec);
        if (bitmap is null)
            return MediaErrors.ImageCorrupt;

        var variants = new List<ProcessedVariant>(MediaConstraints.VariantsPerObject);

        foreach (var (variant, maxSide) in new[]
                 {
                     (MediaVariantName.Thumb, MediaConstraints.ThumbMaxSide),
                     (MediaVariantName.Display, MediaConstraints.DisplayMaxSide),
                 })
        {
            using var resized = Resize(bitmap, maxSide);
            if (resized is null)
                return MediaErrors.ImageCorrupt;

            using var image = SKImage.FromBitmap(resized);
            using var encoded = image.Encode(SKEncodedImageFormat.Webp, Quality);
            if (encoded is null || encoded.Size == 0)
                return MediaErrors.ImageCorrupt;

            variants.Add(new ProcessedVariant(variant, encoded.ToArray(), resized.Width, resized.Height));
        }

        return new ProcessedImage(probe.Format.Value, bitmap.Width, bitmap.Height, variants);
    }

    /// <summary>Уменьшает изображение до предела длинной стороны, сохраняя пропорции.</summary>
    private static SKBitmap? Resize(SKBitmap source, int maxSide)
    {
        var scale = Math.Min(1d, (double)maxSide / Math.Max(source.Width, source.Height));
        var width = Math.Max(1, (int)Math.Round(source.Width * scale));
        var height = Math.Max(1, (int)Math.Round(source.Height * scale));

        var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        return source.Resize(info, new SKSamplingOptions(SKCubicResampler.Mitchell));
    }
}
