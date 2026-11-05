using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Tests.Integration.Helpers;

/// <summary>
/// Поддельный token-endpoint Keycloak для typed client KeycloakTokenClient.
/// Отвечает функцией <see cref="Respond"/> и запоминает тела запросов.
/// </summary>
public sealed class FakeKeycloak : HttpMessageHandler
{
    public ConcurrentQueue<string> RequestBodies { get; } = new();

    public int RequestCount => RequestBodies.Count;

    /// <summary>Ответ на очередной запрос. По умолчанию успешный refresh с новыми токенами.</summary>
    public Func<HttpRequestMessage, Task<HttpResponseMessage>> Respond { get; set; } =
        _ => Task.FromResult(Success());

    public static HttpResponseMessage Success(
        string? accessToken = null,
        string refreshToken = "refreshed-refresh",
        string? tokenType = "Bearer",
        int expiresIn = 300
    )
    {
        var body = new Dictionary<string, object>
        {
            ["access_token"] = accessToken ?? TestJwt.Create("refreshed-role"),
            ["refresh_token"] = refreshToken,
            ["id_token"] = "refreshed-id",
            ["expires_in"] = expiresIn,
            ["refresh_expires_in"] = 1800,
        };
        if (tokenType is not null)
            body["token_type"] = tokenType;
        return Json(HttpStatusCode.OK, JsonSerializer.Serialize(body));
    }

    public static HttpResponseMessage Json(HttpStatusCode status, string body) =>
        new(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") };

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var body = request.Content is null
            ? string.Empty
            : await request.Content.ReadAsStringAsync(cancellationToken);
        RequestBodies.Enqueue(body);
        return await Respond(request);
    }
}
