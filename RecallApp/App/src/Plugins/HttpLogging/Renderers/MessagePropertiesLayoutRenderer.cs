using System.Text;
using System.Text.Json;
using NLog;
using NLog.LayoutRenderers;

namespace HttpLogging.Renderers;

/// <summary>
/// Рендерит структурированные параметры лог-события как JSON (без служебных ключей плагина).
/// </summary>
[LayoutRenderer("http-message-properties")]
public sealed class MessagePropertiesLayoutRenderer : LayoutRenderer
{
    protected override void Append(StringBuilder builder, LogEventInfo logEvent)
    {
        var props = new Dictionary<string, object?>();
        foreach (var kv in logEvent.Properties)
        {
            if (kv.Key is string key && !key.StartsWith(HttpLoggingPipeline.CachePrefix, StringComparison.Ordinal))
                props[key] = kv.Value;
        }

        builder.Append(JsonSerializer.Serialize(props));
    }
}
