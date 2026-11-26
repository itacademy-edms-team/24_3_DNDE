using System.Globalization;
using System.Text;
using NLog;
using NLog.LayoutRenderers;

namespace HttpLogging.Renderers;

/// <summary>
/// Рендерит типизированное значение источника как строку. NLog сам конвертирует её в CLR-тип
/// колонки по DbType (Int32/Double/DateTimeOffset), заданному в параметре DatabaseTarget.
/// </summary>
[LayoutRenderer("http-value")]
public sealed class TypedValueLayoutRenderer : LayoutRenderer
{
    public string Source { get; set; } = string.Empty;

    protected override void Append(StringBuilder builder, LogEventInfo logEvent)
    {
        var value = HttpLoggingPipeline.GetSanitized(logEvent, Source);
        if (value is null)
            return;

        var text = value switch
        {
            DateTimeOffset dto => dto.ToString("O", CultureInfo.InvariantCulture),
            _ => Convert.ToString(value, CultureInfo.InvariantCulture),
        };

        if (text is not null)
            builder.Append(text);
    }
}
