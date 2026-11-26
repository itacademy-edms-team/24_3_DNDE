using NLog;

namespace HttpLogging;

/// <summary>
/// Настройки access-лога: имя NLog-логгера и минимальный уровень записи в БД.
/// </summary>
public sealed class AccessLogOptions
{
    /// <summary>
    /// Возвращает или задает имя логгера, которым middleware пишет access-запись.
    /// </summary>
    public string LoggerName { get; set; } = "HttpLogging.Access";

    /// <summary>
    /// Возвращает или задает минимальный уровень, с которого события пишутся в БД.
    /// </summary>
    public LogLevel MinLevel { get; set; } = LogLevel.Info;
}
