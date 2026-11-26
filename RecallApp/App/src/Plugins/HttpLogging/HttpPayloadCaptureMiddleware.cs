using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace HttpLogging;

/// <summary>
/// Перехватывает тела запроса/ответа в HttpContext.Items и в конце запроса пишет одну
/// access-запись через NLog (logger HttpLogging.Access). Бинарные тела не логируются.
/// </summary>
internal sealed class HttpPayloadCaptureMiddleware(RequestDelegate next)
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetLogger("HttpLogging.Access");

    public async Task InvokeAsync(HttpContext context)
    {
        context.Items[HttpLoggingKeys.StartTime] = DateTimeOffset.UtcNow;

        var requestBody = await CaptureRequestBodyAsync(context);
        context.Items[HttpLoggingKeys.RequestBody] = requestBody;

        var originalBody = context.Response.Body;
        using var capture = new MemoryStream();
        context.Response.Body = capture;

        var sw = Stopwatch.StartNew();
        Exception? capturedException = null;
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            capturedException = ex;
            throw;
        }
        finally
        {
            sw.Stop();
            var elapsedMs = sw.Elapsed.TotalMilliseconds;
            var statusCode = capturedException is not null 
                ? 500 
                : context.Response.StatusCode;

            context.Items[HttpLoggingKeys.ResponseBody] = ReadCapturedResponse(capture, context.Response.ContentType);
            context.Items[HttpLoggingKeys.StatusCode] = statusCode;
            context.Items[HttpLoggingKeys.DurationMs] = elapsedMs;

            capture.Position = 0;
            await capture.CopyToAsync(originalBody);
            context.Response.Body = originalBody;

            var level = capturedException is not null || statusCode >= 500 
                ? NLog.LogLevel.Error
                : statusCode >= 400 
                    ? NLog.LogLevel.Warn
                    : NLog.LogLevel.Info;

            var summary = capturedException is not null || statusCode >= 500
                ? $"HTTP {context.Request.Method} {context.Request.Path} failed with {statusCode} in {elapsedMs:0.0} ms"
                : $"HTTP {context.Request.Method} {context.Request.Path} responded {statusCode} in {elapsedMs:0.0} ms";

            Logger.Log(new NLog.LogEventInfo(level, Logger.Name, summary) { Exception = capturedException });
        }
    }

    private static async Task<string?> CaptureRequestBodyAsync(HttpContext context)
    {
        if (!IsTextual(context.Request.ContentType))
            return null;

        context.Request.EnableBuffering();
        using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);
        var body = await reader.ReadToEndAsync(context.RequestAborted);
        context.Request.Body.Position = 0;
        return body;
    }

    private static string? ReadCapturedResponse(MemoryStream capture, string? contentType)
    {
        if (!IsTextual(contentType) || capture.Length == 0)
            return null;

        return Encoding.UTF8.GetString(capture.GetBuffer(), 0, (int)capture.Length);
    }

    private static bool IsTextual(string? contentType)
    {
        if (string.IsNullOrEmpty(contentType))
            return true;

        return contentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase)
            || contentType.StartsWith("application/problem+json", StringComparison.OrdinalIgnoreCase)
            || contentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase)
            || contentType.StartsWith("application/xml", StringComparison.OrdinalIgnoreCase)
            || contentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase);
    }
}
