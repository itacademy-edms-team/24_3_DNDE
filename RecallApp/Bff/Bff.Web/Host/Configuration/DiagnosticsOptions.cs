namespace Host.Configuration;

/// <summary>
/// Настройки диагностических маршрутов (секция "Diagnostics").
/// Включаются явным флагом.
/// </summary>
public sealed class DiagnosticsOptions
{
    public const string SectionName = "Diagnostics";

    /// <summary>
    /// GET /debug/session: снимок того, что BFF знает о сессии, включая роли из access-токена.
    /// </summary>
    public bool Session { get; set; }
}
