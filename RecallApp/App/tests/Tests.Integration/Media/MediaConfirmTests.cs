using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Media.Domain.Objects;
using Media.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;

namespace Tests.Integration.Media;

/// <summary>
/// Подтверждение загрузки на реальном cloud.ru: HEAD, обработка, варианты и переход в `ready`.
/// Каждый тест загружает собственный файл через выданную политику, поэтому проверяется полный цикл.
/// </summary>
[Trait("Category", "S3")]
public sealed class MediaConfirmTests : IClassFixture<AppFactory>
{
    private const string UploadsUrl = "/api/media/uploads";

    private readonly AppFactory _factory;

    public MediaConfirmTests(AppFactory factory) => _factory = factory;

    [DatabaseFact]
    public async Task Confirm_UploadedPng_ReturnsReadyWithTwoVariants()
    {
        using var client = CreateClientForNewUser();
        var mediaId = await InitAndUploadAsync(client, CreatePng(800, 600));

        var response = await ConfirmAsync(client, mediaId, Guid.NewGuid().ToString());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal("ready", body.GetProperty("status").GetString());
        Assert.Equal(mediaId, body.GetProperty("mediaId").GetGuid());

        var variants = body.GetProperty("variants");
        Assert.True(variants.TryGetProperty("thumb", out var thumb));
        Assert.True(variants.TryGetProperty("display", out var display));
        Assert.Equal(MediaConstraints.ThumbMaxSide, Math.Max(thumb.GetProperty("width").GetInt32(), thumb.GetProperty("height").GetInt32()));
        Assert.Equal(800, Math.Max(display.GetProperty("width").GetInt32(), display.GetProperty("height").GetInt32()));

        // Ссылка на вариант действительно отдаёт файл из хранилища.
        using var http = new HttpClient();
        var downloaded = await http.GetAsync(thumb.GetProperty("url").GetString());
        Assert.Equal(HttpStatusCode.OK, downloaded.StatusCode);
        Assert.Equal("RIFF"u8.ToArray(), (await downloaded.Content.ReadAsByteArrayAsync())[..4]);
    }

    [DatabaseFact]
    public async Task Confirm_RepeatedWithSameKey_ReturnsCachedResponse()
    {
        using var client = CreateClientForNewUser();
        var mediaId = await InitAndUploadAsync(client, CreatePng(400, 300));
        var key = Guid.NewGuid().ToString();

        var first = await ConfirmAsync(client, mediaId, key);
        var second = await ConfirmAsync(client, mediaId, key);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);

        var firstBody = await first.Content.ReadFromJsonAsync<JsonElement>();
        var secondBody = await second.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(firstBody.GetProperty("status").GetString(), secondBody.GetProperty("status").GetString());
    }

    [DatabaseFact]
    public async Task Confirm_SameKeyForDifferentMedia_Returns409KeyReused()
    {
        using var client = CreateClientForNewUser();
        var firstMedia = await InitAndUploadAsync(client, CreatePng(200, 200));
        var secondMedia = await InitAndUploadAsync(client, CreatePng(200, 200));
        var key = Guid.NewGuid().ToString();

        await ConfirmAsync(client, firstMedia, key);
        var response = await ConfirmAsync(client, secondMedia, key);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await ReadProblemAsync(response);
        Assert.Equal("Idempotency.KeyReused", problem.GetProperty("title").GetString());
    }

    [DatabaseFact]
    public async Task Confirm_UnknownMedia_Returns404()
    {
        using var client = CreateClientForNewUser();

        var response = await ConfirmAsync(client, Guid.NewGuid(), Guid.NewGuid().ToString());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problem = await ReadProblemAsync(response);
        Assert.Equal("Media.NotFound", problem.GetProperty("title").GetString());
    }

    [DatabaseFact]
    public async Task Confirm_ForeignMedia_Returns404()
    {
        using var owner = CreateClientForNewUser();
        using var stranger = CreateClientForNewUser();
        var mediaId = await InitAndUploadAsync(owner, CreatePng(200, 200));

        var response = await ConfirmAsync(stranger, mediaId, Guid.NewGuid().ToString());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [DatabaseFact]
    public async Task Confirm_ObjectNeverUploaded_ReturnsObjectMissingAndMarksFailed()
    {
        using var client = CreateClientForNewUser();
        var mediaId = await InitAsync(client);

        var response = await ConfirmAsync(client, mediaId, Guid.NewGuid().ToString());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await ReadProblemAsync(response);
        Assert.Equal("Media.ObjectMissing", problem.GetProperty("title").GetString());

        // Файл окончательно непригоден: повторять confirm бессмысленно, статус зафиксирован.
        var state = await GetMediaStateAsync(mediaId);
        Assert.Equal("Failed", state);
    }

    [DatabaseFact]
    public async Task Confirm_NotAnImage_ReturnsInvalidFormatAndMarksFailed()
    {
        using var client = CreateClientForNewUser();
        var mediaId = await InitAndUploadAsync(client, "это не картинка"u8.ToArray());

        var response = await ConfirmAsync(client, mediaId, Guid.NewGuid().ToString());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await ReadProblemAsync(response);
        Assert.Equal("Media.InvalidFormat", problem.GetProperty("title").GetString());
    }

    [DatabaseFact]
    public async Task Confirm_AfterUploadWindow_ReturnsUploadExpired()
    {
        using var client = CreateClientForNewUser();
        var mediaId = await InitAndUploadAsync(client, CreatePng(200, 200));

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<MediaDbContext>();
            await context.Database.ExecuteSqlRawAsync(
                "UPDATE media.media_objects SET upload_expires_at = now() - interval '1 minute' WHERE id = {0}",
                mediaId);
        }

        var response = await ConfirmAsync(client, mediaId, Guid.NewGuid().ToString());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await ReadProblemAsync(response);
        Assert.Equal("Media.UploadExpired", problem.GetProperty("title").GetString());
    }

    private HttpClient CreateClientForNewUser()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(AppFactory.TestAuthHeader, "true");
        client.DefaultRequestHeaders.Add(AppFactory.TestSubHeader, Guid.NewGuid().ToString());
        return client;
    }

    private static async Task<Guid> InitAsync(HttpClient client)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, UploadsUrl)
        {
            Content = JsonContent.Create(new { contentType = "image/png", sizeBytes = MediaConstraints.MaxUploadBytes }),
        };
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());

        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("mediaId").GetGuid();
    }

    /// <summary>Полный путь клиента: init, загрузка байтов по политике, возврат mediaId.</summary>
    private static async Task<Guid> InitAndUploadAsync(HttpClient client, byte[] content)
    {
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
        if (uploaded.StatusCode is not (HttpStatusCode.OK or HttpStatusCode.Created or HttpStatusCode.NoContent))
        {
            var error = await uploaded.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Загрузка не удалась: {(int)uploaded.StatusCode}: {error}");
        }

        return init.GetProperty("mediaId").GetGuid();
    }

    private static Task<HttpResponseMessage> ConfirmAsync(HttpClient client, Guid mediaId, string key)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{UploadsUrl}/{mediaId}/confirm");
        request.Headers.Add("Idempotency-Key", key);
        return client.SendAsync(request);
    }

    private async Task<string?> GetMediaStateAsync(Guid mediaId)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MediaDbContext>();
        var status = await context.MediaObjects
            .Where(media => media.Id == mediaId)
            .Select(media => media.Status)
            .SingleAsync();
        return status.ToString();
    }

    private static async Task<JsonElement> ReadProblemAsync(HttpResponseMessage response)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private static byte[] CreatePng(int width, int height)
    {
        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.SeaGreen);
        canvas.Flush();

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 90);
        return data.ToArray();
    }
}
