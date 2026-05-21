using System.Net.Http.Headers;

namespace FinanceTrack.Finance.Infrastructure.Ai;

// Yandex AI Studio expects "Api-Key <key>", but the OpenAI SDK sends "Bearer <key>".
// This handler replaces the Authorization header before the request leaves the process.
internal sealed class YandexAiAuthHandler(string apiKey) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancel
    )
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Api-Key", apiKey);
        return base.SendAsync(request, cancel);
    }
}
