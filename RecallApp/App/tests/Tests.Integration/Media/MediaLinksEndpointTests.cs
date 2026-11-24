using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using SkiaSharp;

namespace Tests.Integration.Media;

/// <summary>
/// Батч-подпись ссылок на чтение (POST /api/media/links): клиент продлевает адреса картинок
/// страницы одним запросом, не перезагружая контент, который их использует.
/// </summary>
[Trait("Category", "S3")]
public sealed class MediaLinksEndpointTests : IClassFixture<AppFactory>
{
    private const string UploadsUrl = "/api/media/uploads";
    private const string LinksUrl = "/api/media/links";

    private readonly AppFactory _factory;

    public MediaLinksEndpointTests(AppFactory factory) => _factory = factory;

    [DatabaseFact]
    public async Task Links_ReadyFiles_ReturnsSignedUrlsForBothVariants()
    {
        using var client = CreateClientForNewUser();
        var first = await InitAndUploadAsync(client);
        var second = await InitAndUploadAsync(client);
        await ConfirmOkAsync(client, first);
        await ConfirmOkAsync(client, second);

        var body = await PostLinksAsync(client, [first, second]);

        var links = body.GetProperty("links");
        Assert.Equal(2, links.EnumerateObject().Count());

        var variants = links.GetProperty(first.ToString()).GetProperty("variants");
        var thumbUrl = variants.GetProperty("thumb").GetProperty("url").GetString();

        using var http = new HttpClient();
        var downloaded = await http.GetAsync(thumbUrl);
        Assert.Equal(HttpStatusCode.OK, downloaded.StatusCode);
        Assert.True(variants.TryGetProperty("display", out _));
    }

    [DatabaseFact]
    public async Task Links_ForeignAndUnknownIds_AreAbsentFromResult()
    {
        using var owner = CreateClientForNewUser();
        using var stranger = CreateClientForNewUser();
        var mediaId = await InitAndUploadAsync(owner);
        await ConfirmOkAsync(owner, mediaId);

        var body = await PostLinksAsync(stranger, [mediaId, Guid.NewGuid()]);

        Assert.Empty(body.GetProperty("links").EnumerateObject());
    }

    [DatabaseFact]
    public async Task Links_PendingFile_IsAbsentFromResult()
    {
        using var client = CreateClientForNewUser();
        var mediaId = await InitAsync(client);

        var body = await PostLinksAsync(client, [mediaId]);

        Assert.Empty(body.GetProperty("links").EnumerateObject());
    }

    [DatabaseFact]
    public async Task Links_EmptyAndMissingList_ReturnsEmptyMap()
    {
        using var client = CreateClientForNewUser();

        var empty = await PostLinksRawAsync(client, """{"mediaIds":[]}""");
        Assert.Equal(HttpStatusCode.OK, empty.StatusCode);
        var withoutField = await PostLinksRawAsync(client, "{}");

        Assert.Equal(HttpStatusCode.OK, withoutField.StatusCode);
        var body = await withoutField.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Empty(body.GetProperty("links").EnumerateObject());
    }

    [DatabaseFact]
    public async Task Links_TooManyIds_Returns400WithLimitCode()
    {
        using var client = CreateClientForNewUser();
        var ids = Enumerable.Range(0, 101).Select(_ => Guid.NewGuid()).ToArray();

        var response = await PostLinksRawAsync(client, JsonSerializer.Serialize(new { mediaIds = ids }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Media.LinksLimitExceeded", problem.GetProperty("title").GetString());
    }

    private static async Task<JsonElement> PostLinksAsync(HttpClient client, IReadOnlyCollection<Guid> mediaIds)
    {
        var response = await PostLinksRawAsync(client, JsonSerializer.Serialize(new { mediaIds }));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private static Task<HttpResponseMessage> PostLinksRawAsync(HttpClient client, string json)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, LinksUrl)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
        };
        return client.SendAsync(request);
    }

    private HttpClient CreateClientForNewUser()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(AppFactory.TestAuthHeader, "true");
        client.DefaultRequestHeaders.Add(AppFactory.TestSubHeader, Guid.NewGuid().ToString());
        return client;
    }

    private static async Task ConfirmOkAsync(HttpClient client, Guid mediaId)
    {
        var confirmed = await ConfirmAsync(client, mediaId);
        Assert.Equal(HttpStatusCode.OK, confirmed.StatusCode);
    }

    private static Task<HttpResponseMessage> ConfirmAsync(HttpClient client, Guid mediaId)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{UploadsUrl}/{mediaId}/confirm");
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        return client.SendAsync(request);
    }

    private static async Task<Guid> InitAsync(HttpClient client)
    {
        var content = CreatePng();
        var initRequest = new HttpRequestMessage(HttpMethod.Post, UploadsUrl)
        {
            Content = JsonContent.Create(new { contentType = "image/png", sizeBytes = content.Length }),
        };
        initRequest.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());

        var initResponse = await client.SendAsync(initRequest);
        Assert.Equal(HttpStatusCode.Created, initResponse.StatusCode);
        var init = await initResponse.Content.ReadFromJsonAsync<JsonElement>();
        return init.GetProperty("mediaId").GetGuid();
    }

    private static async Task<Guid> InitAndUploadAsync(HttpClient client)
    {
        var content = CreatePng();
        var initRequest = new HttpRequestMessage(HttpMethod.Post, UploadsUrl)
        {
            Content = JsonContent.Create(new { contentType = "image/png", sizeBytes = content.Length }),
        };
        initRequest.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());

        var initResponse = await client.SendAsync(initRequest);
        Assert.Equal(HttpStatusCode.Created, initResponse.StatusCode);
        var init = await initResponse.Content.ReadFromJsonAsync<JsonElement>();

        using var form = new MultipartFormDataContent();
        foreach (var field in init.GetProperty("fields").EnumerateObject())
            form.Add(new StringContent(field.Value.GetString() ?? string.Empty), field.Name);

        var file = new ByteArrayContent(content);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(file, "file", "image.png");

        using var http = new HttpClient();
        var uploaded = await http.PostAsync(init.GetProperty("url").GetString(), form);
        Assert.True(
            uploaded.StatusCode is HttpStatusCode.OK or HttpStatusCode.Created or HttpStatusCode.NoContent,
            $"Загрузка не удалась: {(int)uploaded.StatusCode}");

        return init.GetProperty("mediaId").GetGuid();
    }

    private static byte[] CreatePng()
    {
        using var bitmap = new SKBitmap(300, 200);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.MediumPurple);
        canvas.Flush();

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 90);
        return data.ToArray();
    }
}
