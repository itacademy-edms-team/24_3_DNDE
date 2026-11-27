using System.Text;
using NLog;
using NLog.LayoutRenderers;

namespace HttpLogging.Renderers;

/// <summary>
/// Рендерит тип или сообщение корневой (первопричинной) ошибки.
/// </summary>
[LayoutRenderer("http-root-exception")]
public sealed class RootExceptionLayoutRenderer : LayoutRenderer
{
    public string Format { get; set; } = "Type";

    protected override void Append(StringBuilder builder, LogEventInfo logEvent)
    {
        var root = logEvent.Exception?.GetBaseException();
        if (root is null)
            return;

        var text = Format.Equals("Message", StringComparison.OrdinalIgnoreCase)
            ? root.Message
            : root.GetType().FullName ?? root.GetType().Name;

        builder.Append(text);
    }
}
