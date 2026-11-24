using System.Net;
using System.Net.Http.Headers;
using Media.Application.Storage;
using Media.Domain.Objects;
using Media.Infrastructure.Storage;
using Tests.Integration.Helpers;

namespace Tests.Integration.Media.Storage;

/// <summary>
/// Хранилище на реальном cloud.ru: политика загрузки, чтение, запись и удаление.
/// Тесты идут на настоящем бакете (ADR 0037), ключи — из окружения; без них — явный провал.
/// </summary>
[Trait("Category", "S3")]
public sealed class S3ObjectStorageTests
{
    private const string PngContentType = "image/png";

    private static readonly byte[] SmallPng = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");

    [Fact]
    public async Task CreateUploadPolicy_ReturnsSignedFieldsAndFixedKey()
    {
        using var scope = new StorageScope();
        var key = scope.NewKey();

        var policy = await scope.Storage.CreateUploadPolicyAsync(
            key, PngContentType, MediaConstraints.MaxUploadBytes, DateTime.UtcNow, CancellationToken.None);

        Assert.False(policy.IsError);
        Assert.False(string.IsNullOrWhiteSpace(policy.Value.Url));
        Assert.Equal(key, policy.Value.ObjectKey);
        Assert.Contains("Policy", policy.Value.Fields.Keys);
        Assert.Contains("x-amz-signature", policy.Value.Fields.Keys);
        Assert.Equal(key, policy.Value.Fields["key"]);
    }

    [Fact]
    public async Task UploadPolicy_RejectsObjectAboveDeclaredMaximum()
    {
        using var scope = new StorageScope();
        var key = scope.NewKey();

        // Лимит политики намеренно маленький: проверяем enforcement хранилищем, а не мегабайтами.
        var policy = await scope.CreateUploadPolicyAsync(
            key, PngContentType, maxBytes: 100, DateTime.UtcNow, CancellationToken.None);
        var oversized = new byte[1024];

        var status = await scope.UploadAsync(policy, oversized);

        Assert.False(status is HttpStatusCode.OK or HttpStatusCode.NoContent or HttpStatusCode.Created,
            $"Загрузка сверх лимита прошла: {(int)status}");
        Assert.Null(await scope.Storage.HeadAsync(key, CancellationToken.None));
    }

    [Fact]
    public async Task WriteThenRead_RoundTripsBytesAndMetadata()
    {
        using var scope = new StorageScope();
        var key = scope.NewKey();

        var written = await scope.Storage.WriteAsync(key, SmallPng, PngContentType, CancellationToken.None);
        Assert.False(written.IsError);

        var head = await scope.Storage.HeadAsync(key, CancellationToken.None);
        Assert.NotNull(head);
        Assert.Equal(SmallPng.Length, head.SizeBytes);

        var read = await scope.Storage.ReadAsync(key, MediaConstraints.MaxUploadBytes, CancellationToken.None);
        Assert.False(read.IsError);
        Assert.Equal(SmallPng, read.Value);
    }

    [Fact]
    public async Task Read_MissingObject_ReturnsObjectMissing()
    {
        using var scope = new StorageScope();

        var read = await scope.Storage.ReadAsync(scope.NewKey(), MediaConstraints.MaxUploadBytes, CancellationToken.None);

        Assert.True(read.IsError);
        Assert.Equal("Media.ObjectMissing", read.FirstError.Code);
    }

    [Fact]
    public async Task CreateReadUrl_DownloadsObject()
    {
        using var scope = new StorageScope();
        var key = scope.NewKey();
        await scope.Storage.WriteAsync(key, SmallPng, PngContentType, CancellationToken.None);

        var url = await scope.Storage.CreateReadUrlAsync(key, MediaConstraints.ReadLinkLifetime, CancellationToken.None);

        Assert.False(url.IsError);
        using var http = new HttpClient();
        var response = await http.GetAsync(url.Value);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(SmallPng, await response.Content.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task Delete_RemovesObject_AndRepeatedDeleteSucceeds()
    {
        using var scope = new StorageScope();
        var key = scope.NewKey();
        await scope.Storage.WriteAsync(key, SmallPng, PngContentType, CancellationToken.None);

        Assert.False((await scope.Storage.DeleteAsync(key, CancellationToken.None)).IsError);
        Assert.Null(await scope.Storage.HeadAsync(key, CancellationToken.None));

        // Идемпотентность: повторное удаление не ошибка.
        Assert.False((await scope.Storage.DeleteAsync(key, CancellationToken.None)).IsError);
    }

    /// <summary>Изолированный префикс теста: каждый прогон убирает за собой.</summary>
    private sealed class StorageScope : IDisposable
    {
        private readonly S3TestSettings _settings;
        private readonly string _prefix;

        public StorageScope()
        {
            _settings = S3TestSettings.FromEnvironment();
            _prefix = $"tests/{Guid.NewGuid():N}";
            Storage = new S3ObjectStorage(
                S3TestEnvironment.CreateClient(_settings),
                _settings.Bucket);
        }

        public IObjectStorage Storage { get; }

        public string NewKey() => $"{_prefix}/{Guid.NewGuid():N}.png";

        public async Task<UploadPolicy> CreateUploadPolicyAsync(
            string key,
            string contentType,
            long maxBytes,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var result = await Storage.CreateUploadPolicyAsync(key, contentType, maxBytes, now, cancellationToken);
            if (result.IsError)
                throw new InvalidOperationException($"Политика не создана: {result.FirstError.Code}");
            return result.Value;
        }

        public async Task<HttpStatusCode> UploadAsync(UploadPolicy policy, byte[] content)
        {
            using var form = new MultipartFormDataContent();
            foreach (var (name, value) in policy.Fields)
                form.Add(new StringContent(value), name);

            var file = new ByteArrayContent(content);
            file.Headers.ContentType = new MediaTypeHeaderValue(PngContentType);
            form.Add(file, "file", "image.png");

            using var http = new HttpClient();
            var response = await http.PostAsync(policy.Url, form);
            return response.StatusCode;
        }

        public void Dispose()
        {
            // Уборка best-effort: тесты не должны падать из-за неубранного файла, но и мусор копить нельзя.
            try
            {
                using var client = S3TestEnvironment.CreateClient(_settings);
                var listed = client.ListObjectsV2Async(new Amazon.S3.Model.ListObjectsV2Request
                {
                    BucketName = _settings.Bucket,
                    Prefix = _prefix + "/",
                }).GetAwaiter().GetResult();

                foreach (var item in listed.S3Objects ?? [])
                    client.DeleteObjectAsync(_settings.Bucket, item.Key).GetAwaiter().GetResult();
            }
            catch
            {
                // Намеренно не мешаем результату теста: уборка повторится следующим прогоном.
            }
        }
    }
}
