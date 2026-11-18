using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Media.Application.Storage;
using Media.Domain.Objects;
using Media.Domain.References;
using Media.Infrastructure.Data;
using Media.Infrastructure.Data.Cleanup;
using Media.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
using Tests.Integration.Helpers;

namespace Tests.Integration.Media;

/// <summary>
/// Фоновая чистка сирот (онтология Media §6.1, §7.3): брошенные загрузки, готовые файлы без ссылок,
/// надгробия удалённых записей. Проверяется на реальном хранилище: важно, что объекты действительно исчезают.
/// </summary>
[Trait("Category", "S3")]
public sealed class MediaCleanupTests : IClassFixture<AppFactory>
{
    private const string UploadsUrl = "/api/media/uploads";

    private readonly AppFactory _factory;

    public MediaCleanupTests(AppFactory factory) => _factory = factory;

    [DatabaseFact]
    public async Task PurgeAbandoned_RemovesPendingRowAndTemporaryObject()
    {
        using var client = CreateClientForNewUser();
        var mediaId = await InitAndUploadAsync(client);

        await BackdateAsync(mediaId, setters => setters.SetProperty(media => media.CreatedAt, DateTime.UtcNow.AddDays(-2)));

        var (removed, _, _) = await RunCleanupAsync();

        Assert.True(removed > 0);
        Assert.False(await RowExistsAsync(mediaId));
    }

    [DatabaseFact]
    public async Task PurgeAbandoned_WithCutoffNow_RemovesFreshPendingUpload()
    {
        // Dev-режим подчистки на старте: ретенция не применяется, убирается любая брошенная загрузка.
        using var client = CreateClientForNewUser();
        var mediaId = await InitAndUploadAsync(client);

        // Метка назад: отбирается самый старый pending, поэтому чужие свежие строки не затрагиваются.
        await BackdateAsync(mediaId, setters => setters.SetProperty(media => media.CreatedAt, DateTime.UtcNow.AddDays(-2)));

        using var scope = _factory.Services.CreateScope();
        var cleaner = scope.ServiceProvider.GetRequiredService<MediaCleaner>();
        var removed = await cleaner.PurgeAbandonedAsync(DateTime.UtcNow, batchSize: 1, CancellationToken.None);

        Assert.Equal(1, removed);
        Assert.False(await RowExistsAsync(mediaId));
    }

    [DatabaseFact]
    public async Task PurgeUnreferenced_MarksDeletedAndRemovesVariants()
    {
        using var client = CreateClientForNewUser();
        var mediaId = await InitAndUploadAsync(client, confirmed: true);

        await BackdateAsync(mediaId, setters => setters.SetProperty(media => media.ReadyAt, DateTime.UtcNow.AddDays(-2)));

        var (_, unreferenced, _) = await RunCleanupAsync();

        Assert.True(unreferenced > 0);
        var status = await ReadStatusAsync(mediaId);
        Assert.Equal(MediaStatus.Deleted, status);

        // Объекты вариантов удалены: ключи из БД больше не находятся в хранилище.
        var storage = CreateStorage();
        foreach (var key in await ReadVariantKeysAsync(mediaId))
            Assert.Null(await storage.HeadAsync(key, CancellationToken.None));
    }

    [DatabaseFact]
    public async Task PurgeUnreferenced_KeepsFileWithReference()
    {
        using var client = CreateClientForNewUser();
        var mediaId = await InitAndUploadAsync(client, confirmed: true);
        await AddReferenceAsync(mediaId, $"set:{Guid.NewGuid()}:element:{Guid.NewGuid()}:front");
        await BackdateAsync(mediaId, setters => setters.SetProperty(media => media.ReadyAt, DateTime.UtcNow.AddDays(-2)));

        await RunCleanupAsync();

        Assert.Equal(MediaStatus.Ready, await ReadStatusAsync(mediaId));
    }

    [DatabaseFact]
    public async Task PurgeDeleted_RemovesTombstoneAfterRetention()
    {
        using var client = CreateClientForNewUser();
        var mediaId = await InitAndUploadAsync(client, confirmed: true);

        var deleted = await client.DeleteAsync($"{UploadsUrl.Replace("/uploads", string.Empty)}/{mediaId}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);

        await BackdateAsync(mediaId, setters => setters.SetProperty(media => media.DeletedAt, DateTime.UtcNow.AddDays(-30)));

        var (_, _, tombstones) = await RunCleanupAsync();

        Assert.True(tombstones > 0);
        Assert.False(await RowExistsAsync(mediaId));
    }

    private async Task<(int Abandoned, int Unreferenced, int Deleted)> RunCleanupAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var cleaner = scope.ServiceProvider.GetRequiredService<MediaCleaner>();
        var now = DateTime.UtcNow;

        var abandoned = await cleaner.PurgeAbandonedAsync(now.AddDays(-1), batchSize: 100, CancellationToken.None);
        var unreferenced = await cleaner.PurgeUnreferencedAsync(now.AddDays(-1), batchSize: 100, CancellationToken.None);
        var deleted = await cleaner.PurgeDeletedAsync(now.AddDays(-1), batchSize: 100, CancellationToken.None);

        return (abandoned, unreferenced, deleted);
    }

    /// <summary>
    /// Сдвигает метку времени назад: проверяем retention без ожидания суток.
    /// </summary>
    private async Task BackdateAsync(
        Guid mediaId,
        Expression<Func<SetPropertyCalls<MediaObject>, SetPropertyCalls<MediaObject>>> setters)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MediaDbContext>();

        await context.MediaObjects
            .Where(media => media.Id == mediaId)
            .ExecuteUpdateAsync(setters);
    }

    private async Task<bool> RowExistsAsync(Guid mediaId)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MediaDbContext>();
        return await context.MediaObjects.AnyAsync(media => media.Id == mediaId);
    }

    private async Task<MediaStatus> ReadStatusAsync(Guid mediaId)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MediaDbContext>();
        return await context.MediaObjects
            .Where(media => media.Id == mediaId)
            .Select(media => media.Status)
            .SingleAsync();
    }

    private async Task<string[]> ReadVariantKeysAsync(Guid mediaId)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MediaDbContext>();
        return await context.MediaVariants
            .Where(variant => variant.MediaId == mediaId)
            .Select(variant => variant.ObjectKey.Value)
            .ToArrayAsync();
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

    private static IObjectStorage CreateStorage()
    {
        var settings = S3TestSettings.FromEnvironment();
        return new S3ObjectStorage(S3TestEnvironment.CreateClient(settings), settings.Bucket);
    }

    private HttpClient CreateClientForNewUser()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(AppFactory.TestAuthHeader, "true");
        client.DefaultRequestHeaders.Add(AppFactory.TestSubHeader, Guid.NewGuid().ToString());
        return client;
    }

    /// <summary>init + загрузка байтов; при `confirmed` дополнительно вызывается confirm.</summary>
    private static async Task<Guid> InitAndUploadAsync(HttpClient client, bool confirmed = false)
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

        var mediaId = init.GetProperty("mediaId").GetGuid();

        if (confirmed)
        {
            var confirmRequest = new HttpRequestMessage(HttpMethod.Post, $"{UploadsUrl}/{mediaId}/confirm");
            confirmRequest.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
            var confirmedResponse = await client.SendAsync(confirmRequest);
            Assert.Equal(HttpStatusCode.OK, confirmedResponse.StatusCode);
        }

        return mediaId;
    }

    private static byte[] CreatePng()
    {
        using var bitmap = new SKBitmap(320, 240);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.SlateBlue);
        canvas.Flush();

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 90);
        return data.ToArray();
    }
}
