using System.Text;
using HttpLogging.Formatters;
using NLog;
using NLog.LayoutRenderers;

namespace HttpLogging.Renderers;

/// <summary>
/// Рендерит маскированное значение источника как строку (опционально обрезанную).
/// </summary>
[LayoutRenderer("http-text")]
public sealed class SanitizedTextLayoutRenderer : LayoutRenderer
{
    public string Source { get; set; } = string.Empty;

    public int MaxLength { get; set; }

    protected override void Append(StringBuilder builder, LogEventInfo logEvent)
    {
        var sanitized = HttpLoggingPipeline.GetSanitized(logEvent, Source);
        var text = new TruncateFormatter(MaxLength).Format(sanitized);
        if (text is not null)
            builder.Append(text);
    }
}
