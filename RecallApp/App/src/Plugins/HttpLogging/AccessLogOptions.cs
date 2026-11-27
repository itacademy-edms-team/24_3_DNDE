using NLog;
using NLog.Targets.Wrappers;

namespace HttpLogging;

/// <summary>
/// Настройки access-лога.
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

    /// <summary>
    /// Возвращает или задает максимальный размер очереди асинхронной записи.
    /// </summary>
    public int QueueLimit { get; set; } = 10000;

    /// <summary>
    /// Возвращает или задает поведение при переполнении очереди асинхронной записи.
    /// </summary>
    public AsyncTargetWrapperOverflowAction OverflowAction { get; set; } = AsyncTargetWrapperOverflowAction.Discard;
}
