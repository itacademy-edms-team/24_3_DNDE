using Media.Domain.Objects;
using Media.Infrastructure.Images;
using SkiaSharp;

namespace Tests.Integration.Media.Images;

/// <summary>
/// Конвейер обработки: лимиты до декодирования, перекодирование в WebP, варианты `thumb`/`display`.
/// Изображения генерируются в тесте, сеть не нужна.
/// </summary>
public sealed class SkiaImageProcessorTests
{
    private readonly SkiaImageProcessor _processor = new();

    [Fact]
    public async Task ProcessAsync_SmallPng_ReturnsWebpVariantsWithinLimits()
    {
        var png = CreatePng(width: 800, height: 600);

        var result = await _processor.ProcessAsync(png, CancellationToken.None);

        Assert.False(result.IsError);
        var image = result.Value;

        Assert.Equal(MediaFormat.Png, image.Format);
        Assert.Equal(800, image.Width);
        Assert.Equal(600, image.Height);
        Assert.Equal(2, image.Variants.Count);

        var thumb = image.Variants.Single(variant => variant.Variant == MediaVariantName.Thumb);
        var display = image.Variants.Single(variant => variant.Variant == MediaVariantName.Display);

        // Пропорции сохранены, длинная сторона в пределах своего варианта.
        Assert.Equal(400, Math.Max(thumb.Width, thumb.Height));
        Assert.Equal(300, Math.Min(thumb.Width, thumb.Height));
        Assert.Equal(800, Math.Max(display.Width, display.Height));

        // Перекодировано в WebP: сигнатура RIFF/WEBP, а не исходный PNG.
        Assert.Equal("RIFF"u8.ToArray(), thumb.Content[..4]);
        Assert.Equal("WEBP"u8.ToArray(), thumb.Content[8..12]);
    }

    [Fact]
    public async Task ProcessAsync_LargeImage_ScalesDisplayDownToLimit()
    {
        var png = CreatePng(width: 3000, height: 2000);

        var result = await _processor.ProcessAsync(png, CancellationToken.None);

        Assert.False(result.IsError);
        var display = result.Value.Variants.Single(variant => variant.Variant == MediaVariantName.Display);

        Assert.Equal(MediaConstraints.DisplayMaxSide, Math.Max(display.Width, display.Height));
        Assert.Equal(1067, Math.Min(display.Width, display.Height));
    }

    [Fact]
    public async Task ProcessAsync_NotAnImage_ReturnsInvalidFormat()
    {
        var result = await _processor.ProcessAsync("не картинка"u8.ToArray(), CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Media.InvalidFormat", result.FirstError.Code);
    }

    [Fact]
    public async Task ProcessAsync_PngSignatureWithBrokenBody_ReturnsImageCorrupt()
    {
        var broken = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D };

        var result = await _processor.ProcessAsync(broken, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Media.ImageCorrupt", result.FirstError.Code);
    }

    [Fact]
    public async Task ProcessAsync_SideAboveLimit_ReturnsResolutionTooLarge()
    {
        var wide = CreatePng(width: MediaConstraints.MaxSide + 1, height: 10);

        var result = await _processor.ProcessAsync(wide, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Media.ResolutionTooLarge", result.FirstError.Code);
    }

    [Fact]
    public async Task ProcessAsync_AnimatedPngMarker_ReturnsUnsupportedAnimation()
    {
        // Сигнатура PNG + чанк acTL: детектор считает файл анимированным до декодирования.
        var animatedMarker = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }
            .Concat("acTL"u8.ToArray())
            .Concat(new byte[32])
            .ToArray();

        var result = await _processor.ProcessAsync(animatedMarker, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Media.UnsupportedAnimation", result.FirstError.Code);
    }

    private static byte[] CreatePng(int width, int height)
    {
        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.CornflowerBlue);
        using var paint = new SKPaint { Color = SKColors.DarkOrange };
        canvas.DrawRect(0, 0, width / 2f, height / 2f, paint);
        canvas.Flush();

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 90);
        return data.ToArray();
    }
}
