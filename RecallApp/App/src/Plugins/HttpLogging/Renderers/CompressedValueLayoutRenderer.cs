using System.Text;
using HttpLogging.Formatters;
using NLog;
using NLog.LayoutRenderers;

namespace HttpLogging.Renderers;

/// <summary>
/// Рендерит маскированное значение источника, сжатое GZip и закодированное в Base64.
/// В SQL значение декодируется через decode(@p, 'base64') в bytea.
/// </summary>
[LayoutRenderer("http-gzip")]
public sealed class CompressedValueLayoutRenderer : LayoutRenderer
{
    private static readonly GzipBase64Formatter Formatter = new();

    public string Source { get; set; } = string.Empty;

    protected override void Append(StringBuilder builder, LogEventInfo logEvent)
    {
        var sanitized = HttpLoggingPipeline.GetSanitized(logEvent, Source);
        var text = Formatter.Format(sanitized);
        if (text is not null)
            builder.Append(text);
    }
}
