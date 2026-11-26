using System.Diagnostics;
using System.Text;
using HttpLogging.Abstractions;
using Microsoft.AspNetCore.Http;

namespace HttpLogging;

/// <summary>
/// Перехватывает тела запроса/ответа в HttpContext.Items и в конце запроса пишет одну
/// access-запись через NLog (logger HttpLogging.Access).
/// </summary>
/// <remarks>
/// Захват настраивается через <see cref="BodyCaptureOptions"/>,
/// фильтрация - через <see cref="IRequestFilter"/>.
/// </remarks>
internal sealed class HttpPayloadCaptureMiddleware(RequestDelegate next, BodyCaptureOptions options, IRequestFilter filter)
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetLogger("HttpLogging.Access");

    public async Task InvokeAsync(HttpContext context)
    {
        if (!filter.ShouldLog(context))
        {
            await next(context);
            return;
        }

        context.Items[HttpLoggingKeys.StartTime] = DateTimeOffset.UtcNow;

        context.Items[HttpLoggingKeys.RequestBody] = await CaptureRequestBodyAsync(context);

        var originalBody = context.Response.Body;
        using var capture = options.CaptureResponseBody ? new MemoryStream() : null;
        if (capture is not null)
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

            if (capture is not null)
            {
                context.Items[HttpLoggingKeys.ResponseBody] = ReadCapturedResponse(capture, context.Response.ContentType);
                capture.Position = 0;
                await capture.CopyToAsync(originalBody);
                context.Response.Body = originalBody;
            }

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

    private async Task<string?> CaptureRequestBodyAsync(HttpContext context)
    {
        if (!options.CaptureRequestBody || !ShouldCapture(context.Request.ContentType))
            return null;

        context.Request.EnableBuffering();
        var body = await ReadBodyAsync(context.Request.Body, options.MaxLength, context.RequestAborted);
        context.Request.Body.Position = 0;
        return body;
    }

    private string? ReadCapturedResponse(MemoryStream capture, string? contentType)
    {
        if (!ShouldCapture(contentType) || capture.Length == 0)
            return null;

        var length = options.MaxLength > 0
            ? (int)Math.Min(capture.Length, options.MaxLength)
            : (int)capture.Length;

        return Encoding.UTF8.GetString(capture.GetBuffer(), 0, length);
    }

    private bool ShouldCapture(string? contentType)
    {
        if (string.IsNullOrEmpty(contentType))
            return true;

        foreach (var prefix in options.ContentTypes)
        {
            if (contentType.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static async Task<string?> ReadBodyAsync(Stream body, int maxLength, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(body, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);

        if (maxLength <= 0)
            return await reader.ReadToEndAsync(cancellationToken);

        var buffer = new char[maxLength];
        var read = await reader.ReadBlockAsync(buffer, 0, maxLength);
        return read == 0 ? null : new string(buffer, 0, read);
    }
}
