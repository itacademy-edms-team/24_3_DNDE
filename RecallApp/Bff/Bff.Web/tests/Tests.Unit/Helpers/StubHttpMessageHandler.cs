using System.Net;
using System.Text;

namespace Tests.Unit.Helpers;

/// <summary>
/// Поддельный token-endpoint: отвечает заданной функцией и запоминает запросы вместе с телом.
/// </summary>
internal sealed class StubHttpMessageHandler(
    Func<HttpRequestMessage, Task<HttpResponseMessage>> respond
) : HttpMessageHandler
{
    public List<(HttpRequestMessage Request, string Body)> Requests { get; } = [];

    public static StubHttpMessageHandler Json(HttpStatusCode status, string body) =>
        new(_ =>
            Task.FromResult(
                new HttpResponseMessage(status)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json"),
                }
            )
        );

    public static StubHttpMessageHandler Throws(Exception exception) =>
        new(_ => Task.FromException<HttpResponseMessage>(exception));

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var body = request.Content is null
            ? string.Empty
            : await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        Requests.Add((request, body));
        return await respond(request).ConfigureAwait(false);
    }
}
