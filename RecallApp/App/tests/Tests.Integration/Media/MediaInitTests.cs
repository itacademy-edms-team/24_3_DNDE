using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Media.Domain.Objects;

namespace Tests.Integration.Media;

/// <summary>
/// Инициализация загрузки: контракт ответа, правила команды и идемпотентность (ADR 0037/0038).
/// Тесты пишут строки `pending` в схему media, но в хранилище ничего не отправляют.
/// </summary>
public sealed class MediaInitTests : IClassFixture<AppFactory>
{
    private const string UploadsUrl = "/api/media/uploads";

    private readonly AppFactory _factory;

    public MediaInitTests(AppFactory factory) => _factory = factory;

    [Fact]
    public async Task InitUpload_Anonymous_Returns401()
    {
        var response = await _factory.CreateClient().PostAsync(
            UploadsUrl,
            new StringContent("{}", System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [DatabaseFact]
    public async Task InitUpload_ValidImage_ReturnsPolicyWithFixedKey()
    {
        using var client = CreateClientForNewUser();
        var key = Guid.NewGuid().ToString();

        var response = await PostInitAsync(client, "image/png", 1024, key);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        var mediaId = body.GetProperty("mediaId").GetGuid();
        Assert.NotEqual(Guid.Empty, mediaId);
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("url").GetString()));

        var objectKey = body.GetProperty("objectKey").GetString();
        Assert.Equal($"tmp/{mediaId}", objectKey);

        // Клиент не может выбрать путь: ключ приходит готовым, поля уже подписаны.
        var fields = body.GetProperty("fields");
        Assert.Equal(objectKey, fields.GetProperty("key").GetString());
        Assert.True(fields.TryGetProperty("Policy", out _));

        // Политика живёт 10 минут (ADR 0037).
        Assert.InRange(body.GetProperty("expiresInSeconds").GetInt32(), 540, 600);
    }

    [DatabaseFact]
    public async Task InitUpload_UnsupportedContentType_Returns400WithMediaCode()
    {
        using var client = CreateClientForNewUser();

        var response = await PostInitAsync(client, "image/gif", 1024, Guid.NewGuid().ToString());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await ReadProblemAsync(response);
        Assert.Equal("Media.ContentTypeUnsupported", problem.GetProperty("title").GetString());
    }

    [DatabaseFact]
    public async Task InitUpload_SizeAboveLimit_Returns400WithMediaCode()
    {
        using var client = CreateClientForNewUser();
        const long tooBig = MediaConstraints.MaxUploadBytes + 1;

        var response = await PostInitAsync(client, "image/png", tooBig, Guid.NewGuid().ToString());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await ReadProblemAsync(response);
        Assert.Equal("Media.FileTooLarge", problem.GetProperty("title").GetString());
    }

    [DatabaseFact]
    public async Task InitUpload_ZeroSize_Returns400WithMediaCode()
    {
        using var client = CreateClientForNewUser();

        var response = await PostInitAsync(client, "image/png", 0, Guid.NewGuid().ToString());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await ReadProblemAsync(response);
        Assert.Equal("Media.FileTooLarge", problem.GetProperty("title").GetString());
    }

    [DatabaseFact]
    public async Task InitUpload_WithoutIdempotencyKey_Returns400WithKeyRequired()
    {
        using var client = CreateClientForNewUser();

        var response = await client.PostAsJsonAsync(UploadsUrl, new { contentType = "image/png", sizeBytes = 1024 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await ReadProblemAsync(response);
        Assert.Equal("Idempotency.KeyRequired", problem.GetProperty("title").GetString());
    }

    [DatabaseFact]
    public async Task InitUpload_RepeatedWithSameKey_ReturnsSameMediaId()
    {
        using var client = CreateClientForNewUser();
        var key = Guid.NewGuid().ToString();

        var first = await PostInitAsync(client, "image/jpeg", 2048, key);
        var second = await PostInitAsync(client, "image/jpeg", 2048, key);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);

        var firstBody = await first.Content.ReadFromJsonAsync<JsonElement>();
        var secondBody = await second.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(firstBody.GetProperty("mediaId").GetGuid(), secondBody.GetProperty("mediaId").GetGuid());
    }

    [DatabaseFact]
    public async Task InitUpload_SameKeyDifferentBody_Returns409KeyReused()
    {
        using var client = CreateClientForNewUser();
        var key = Guid.NewGuid().ToString();

        await PostInitAsync(client, "image/png", 1024, key);
        var response = await PostInitAsync(client, "image/webp", 1024, key);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await ReadProblemAsync(response);
        Assert.Equal("Idempotency.KeyReused", problem.GetProperty("title").GetString());
    }

    [DatabaseFact]
    public async Task InitUpload_SixthPending_Returns409PendingLimitReached()
    {
        using var client = CreateClientForNewUser();

        for (var index = 0; index < MediaConstraints.MaxPendingPerOwner; index++)
        {
            var created = await PostInitAsync(client, "image/png", 1024, Guid.NewGuid().ToString());
            Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        }

        var response = await PostInitAsync(client, "image/png", 1024, Guid.NewGuid().ToString());

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await ReadProblemAsync(response);
        Assert.Equal("Media.PendingLimitReached", problem.GetProperty("title").GetString());
    }

    private HttpClient CreateClientForNewUser()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(AppFactory.TestAuthHeader, "true");
        client.DefaultRequestHeaders.Add(AppFactory.TestSubHeader, Guid.NewGuid().ToString());
        return client;
    }

    private static Task<HttpResponseMessage> PostInitAsync(HttpClient client, string? contentType, long? sizeBytes, string? key)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, UploadsUrl)
        {
            Content = JsonContent.Create(new { contentType, sizeBytes }),
        };

        if (key is not null)
            request.Headers.Add("Idempotency-Key", key);

        return client.SendAsync(request);
    }

    private static async Task<JsonElement> ReadProblemAsync(HttpResponseMessage response)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }
}
