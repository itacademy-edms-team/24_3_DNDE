namespace HttpLogging;

/// <summary>
/// Настройки плагина HTTP-логирования.
/// </summary>
public sealed class HttpLoggingOptions
{
    /// <summary>
    /// Имя сервиса, записываемое в колонку service_name.
    /// </summary>
    /// <example>
    /// app, bff, ...
    /// </example>
    public string ServiceName { get; set; } = "app";
}
