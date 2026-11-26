using HttpLogging.Renderers;
using NLog;
using NLog.LayoutRenderers;

namespace HttpLogging;

/// <summary>
/// Предоставляет регистрацию кастомных NLog-рендереров плагина.
/// </summary>
internal static class RendererRegistration
{
    private static bool _registered;

    /// <summary>
    /// Регистрирует рендереры плагина в NLog.
    /// </summary>
    /// <remarks>
    /// Вызов идемпотентен.
    /// </remarks>
    public static void Register()
    {
        if (_registered)
            return;

        _registered = true;

        LogManager.Setup().SetupExtensions(ext =>
        {
            ext.RegisterLayoutRenderer<ActivityTraceLayoutRenderer>("activity");
            ext.RegisterLayoutRenderer<SanitizedTextLayoutRenderer>("http-text");
            ext.RegisterLayoutRenderer<CompressedValueLayoutRenderer>("http-gzip");
            ext.RegisterLayoutRenderer<TypedValueLayoutRenderer>("http-value");
            ext.RegisterLayoutRenderer<RootExceptionLayoutRenderer>("http-root-exception");
            ext.RegisterLayoutRenderer<MessagePropertiesLayoutRenderer>("http-message-properties");
        });
    }
}
