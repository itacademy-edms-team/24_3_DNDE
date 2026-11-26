namespace HttpLogging;

/// <summary>
/// Ключи <see cref="Microsoft.AspNetCore.Http.HttpContext.Items"/>
/// для передачи данных между middleware и NLog-рендерерами.
/// </summary>
internal static class HttpLoggingKeys
{
    public const string StartTime = "__http_logging_start_time";
    public const string RequestBody = "__http_logging_req_body";
    public const string ResponseBody = "__http_logging_resp_body";
    public const string StatusCode = "__http_logging_status_code";
    public const string DurationMs = "__http_logging_duration_ms";
}
