using Media.Domain.Objects;

namespace Tests.Integration.Media.Objects;

/// <summary>
/// Определение формата по сигнатуре - первая защита от подмены типа (ADR 0018).
/// </summary>
public sealed class MediaFormatDetectorTests
{
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] JpegSignature = [0xFF, 0xD8, 0xFF, 0xE0];

    [Fact]
    public void Detect_PngSignature_ReturnsPngStatic()
    {
        var probe = MediaFormatDetector.Detect(PngSignature);

        Assert.Equal(MediaFormat.Png, probe.Format);
        Assert.False(probe.IsAnimated);
    }

    [Fact]
    public void Detect_PngWithActlChunk_ReturnsAnimated()
    {
        var content = PngSignature.Concat("IHDR"u8.ToArray()).Concat("acTL"u8.ToArray()).ToArray();

        var probe = MediaFormatDetector.Detect(content);

        Assert.Equal(MediaFormat.Png, probe.Format);
        Assert.True(probe.IsAnimated);
    }

    [Fact]
    public void Detect_JpegSignature_ReturnsJpeg()
    {
        var probe = MediaFormatDetector.Detect(JpegSignature);

        Assert.Equal(MediaFormat.Jpeg, probe.Format);
        Assert.False(probe.IsAnimated);
    }

    [Fact]
    public void Detect_WebpWithAnimationFlag_ReturnsAnimated()
    {
        var content = WebpHeader(flags: 0x02);

        var probe = MediaFormatDetector.Detect(content);

        Assert.Equal(MediaFormat.Webp, probe.Format);
        Assert.True(probe.IsAnimated);
    }

    [Fact]
    public void Detect_WebpWithoutAnimationFlag_ReturnsStatic()
    {
        var content = WebpHeader(flags: 0x00);

        var probe = MediaFormatDetector.Detect(content);

        Assert.Equal(MediaFormat.Webp, probe.Format);
        Assert.False(probe.IsAnimated);
    }

    [Fact]
    public void Detect_Gif_NotInWhitelist()
    {
        var gif = "GIF89a"u8.ToArray().Concat(new byte[30]).ToArray();

        var probe = MediaFormatDetector.Detect(gif);

        Assert.Null(probe.Format);
    }

    [Fact]
    public void Detect_Svg_NotInWhitelist()
    {
        var svg = "<svg xmlns='http://www.w3.org/2000/svg'><script/></svg>"u8.ToArray();

        Assert.Null(MediaFormatDetector.Detect(svg).Format);
    }

    [Fact]
    public void Detect_EmptyContent_ReturnsNoFormat()
    {
        Assert.Null(MediaFormatDetector.Detect([]).Format);
    }

    /// <summary>Минимальный контейнер RIFF/WEBP с VP8X-заголовком и заданными флагами.</summary>
    private static byte[] WebpHeader(byte flags)
    {
        var content = new byte[30];
        "RIFF"u8.ToArray().CopyTo(content, 0);
        "WEBP"u8.ToArray().CopyTo(content, 8);
        "VP8X"u8.ToArray().CopyTo(content, 12);
        content[20] = flags;
        return content;
    }
}
