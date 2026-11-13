using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Media.Domain.References;
using Media.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;

namespace Tests.Integration.Media;

/// <summary>
/// Чтение состояния файла и удаление:
/// ленивая проверка `pending`, ссылки на варианты (ADR 0017)
/// и запрет удаления используемого файла (онтология Media §6.4).
/// </summary>
[Trait("Category", "S3")]
public sealed class MediaReadDeleteTests : IClassFixture<AppFactory>
{
    private const string UploadsUrl = "/api/media/uploads";

    private readonly AppFactory _factory;

    public MediaReadDeleteTests(AppFactory factory) => _factory = factory;

    [DatabaseFact]
    public async Task Get_UnknownMedia_Returns404()
    {
        using var client = CreateClientForNewUser();

        var response = await client.GetAsync($"{UploadsUrl.Replace("/uploads", string.Empty)}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DatabaseFact]
    public async Task Get_ForeignMedia_Returns404()
    {
        using var owner = CreateClientForNewUser();
        using var stranger = CreateClientForNewUser();
        var mediaId = await InitAndUploadAsync(owner);

        var response = await stranger.GetAsync(MediaUrl(mediaId));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DatabaseFact]
    public async Task Get_ReadyFile_ReturnsVariantsWithLinks()
    {
        using var client = CreateClientForNewUser();
        var mediaId = await InitAndUploadAsync(client);
        await ConfirmAsync(client, mediaId);

        var response = await client.GetAsync(MediaUrl(mediaId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("ready", body.GetProperty("status").GetString());
        Assert.True(body.TryGetProperty("createdAt", out _));

        var thumb = body.GetProperty("variants").GetProperty("thumb");
        using var http = new HttpClient();
        var downloaded = await http.GetAsync(thumb.GetProperty("url").GetString());
        Assert.Equal(HttpStatusCode.OK, downloaded.StatusCode);
    }

    [DatabaseFact]
    public async Task Get_PendingWithoutObject_MarksFailedAndReportsStatus()
    {
        using var client = CreateClientForNewUser();

        // init без загрузки: объекта в хранилище нет, окно доверия ещё открыто.
        var initRequest = new HttpRequestMessage(HttpMethod.Post, UploadsUrl)
        {
            Content = JsonContent.Create(new { contentType = "image/png", sizeBytes = 1024 }),
        };
        initRequest.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var init = await client.SendAsync(initRequest);
        var mediaId = (await init.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("mediaId").GetGuid();

        var response = await client.GetAsync(MediaUrl(mediaId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("failed", body.GetProperty("status").GetString());
        Assert.Equal("Media.ObjectMissing", body.GetProperty("failureCode").GetString());
    }

    [DatabaseFact]
    public async Task Delete_WithoutReferences_Returns204AndMarksDeleted()
    {
        using var client = CreateClientForNewUser();
        var mediaId = await InitAndUploadAsync(client);
        await ConfirmAsync(client, mediaId);

        var deleted = await client.DeleteAsync(MediaUrl(mediaId));
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);

        var state = await client.GetAsync(MediaUrl(mediaId));
        var body = await state.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("deleted", body.GetProperty("status").GetString());

        // Повторное удаление идемпотентно.
        var again = await client.DeleteAsync(MediaUrl(mediaId));
        Assert.Equal(HttpStatusCode.NoContent, again.StatusCode);
    }

    [DatabaseFact]
    public async Task Delete_WithReferences_Returns409InUse()
    {
        using var client = CreateClientForNewUser();
        var mediaId = await InitAndUploadAsync(client);
        await ConfirmAsync(client, mediaId);

        await AddReferenceAsync(mediaId, $"set:{Guid.NewGuid()}:element:{Guid.NewGuid()}:front");

        var response = await client.DeleteAsync(MediaUrl(mediaId));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Media.InUse", problem.GetProperty("title").GetString());

        // Файл остаётся доступным.
        var state = await client.GetAsync(MediaUrl(mediaId));
        var body = await state.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("ready", body.GetProperty("status").GetString());
    }

    [DatabaseFact]
    public async Task Delete_ForeignMedia_Returns404()
    {
        using var owner = CreateClientForNewUser();
        using var stranger = CreateClientForNewUser();
        var mediaId = await InitAndUploadAsync(owner);

        var response = await stranger.DeleteAsync(MediaUrl(mediaId));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task AddReferenceAsync(Guid mediaId, string reference)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MediaDbContext>();

        var created = MediaReference.Create(mediaId, reference, DateTime.UtcNow);
        Assert.False(created.IsError);

        context.Add(created.Value);
        await context.SaveChangesAsync();
    }

    private HttpClient CreateClientForNewUser()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(AppFactory.TestAuthHeader, "true");
        client.DefaultRequestHeaders.Add(AppFactory.TestSubHeader, Guid.NewGuid().ToString());
        return client;
    }

    private static string MediaUrl(Guid mediaId) => $"{UploadsUrl.Replace("/uploads", string.Empty)}/{mediaId}";

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

    private static Task<HttpResponseMessage> ConfirmAsync(HttpClient client, Guid mediaId)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{UploadsUrl}/{mediaId}/confirm");
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        return client.SendAsync(request);
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
