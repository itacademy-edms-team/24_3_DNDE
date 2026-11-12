using System.Net;
using System.Net.Http.Headers;
using Amazon.S3;
using Amazon.S3.Model;
using Tests.Integration.Helpers;

namespace Tests.Integration;

/// <summary>
/// Spike S3 (ADR 0037): проверяем допущения до реализации модуля Media -
/// CreatePresignedPost на реальном cloud.ru, принудительный content-length-range,
/// работа с кастомным ServiceURL + RegionEndpoint, наличие прав editor/viewer.
/// Тесты идут на реальном хранилище, ключи берутся из окружения.
/// </summary>
[Trait("Category", "S3")]
public sealed class S3SpikeTests
{
    private const long MaxUploadBytes = 10 * 1024 * 1024;

    [Fact]
    public async Task PresignedPost_UploadsPng_AndObjectIsReadable()
    {
        using var scope = SpikeScope.Create();
        var key = scope.NewKey("a.png");

        var presignedResponse = await scope.CreatePresignedPostAsync(key, "image/png");
        Assert.False(string.IsNullOrWhiteSpace(presignedResponse.Url));
        Assert.Equal(key, presignedResponse.Fields["key"]);

        try
        {
            var bytes = SmallPng();
            var status = await scope.UploadAsync(presignedResponse, bytes, "image/png");
            Assert.True(status is HttpStatusCode.OK or HttpStatusCode.Created or HttpStatusCode.NoContent,
                $"Ожидали успешный код загрузки, получили {(int)status}");

            // cloud.ru отвечает 200 даже при success_action_status=201, поэтому факт загрузки проверяем HEAD-ом
            var head = await scope.GetObjectMetadataAsync(key);
            Assert.Equal(bytes.Length, head.ContentLength);

            var getUrl = await scope.GetPreSignedURLAsync(key);
            using var http = new HttpClient();
            var getResponse = await http.GetAsync(getUrl);
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        }
        finally
        {
            await scope.DeleteAsync(key);
        }
    }

    [Fact]
    public async Task PresignedPost_RejectsUploadAboveDeclaredMaximum()
    {
        using var scope = SpikeScope.Create();
        var key = scope.NewKey("too-big.png");

        // Лимит в политике намеренно маленький: проверяем именно enforcement, не гоняя мегабайты
        var presigned = await scope.CreatePresignedPostAsync(key, "image/png", maxBytes: 100);
        var oversized = new byte[1024];

        var status = await scope.UploadAsync(presigned, oversized, "image/png");

        Assert.False(status is HttpStatusCode.OK or HttpStatusCode.NoContent,
            $"Загрузка сверх лимита прошла: {(int)status}");

        var missing = await Assert.ThrowsAsync<AmazonS3Exception>(() =>
            scope.GetObjectMetadataAsync(key));
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    [Fact]
    public async Task Bucket_AllowsListObjects()
    {
        using var scope = SpikeScope.Create();

        var response = await scope.ListObjectsV2Async();

        Assert.Equal(HttpStatusCode.OK, response.HttpStatusCode);
    }

    [Fact]
    public async Task Cleanup_RemovesOnlyStaleObjects()
    {
        using var scope = SpikeScope.Create();
        var first = scope.NewKey("a.png");
        var second = scope.NewKey("b.png");
        await scope.UploadPngAsync(first);
        await scope.UploadPngAsync(second);

        // Свежие объекты чистка не трогает
        var fresh = await scope.CleanupStaleAsync(DateTime.UtcNow.AddMinutes(-1));
        Assert.Empty(fresh);
        Assert.Equal(HttpStatusCode.OK, (await scope.GetObjectMetadataAsync(first)).HttpStatusCode);
        Assert.Equal(HttpStatusCode.OK, (await scope.GetObjectMetadataAsync(second)).HttpStatusCode);

        // Всё, что старше заданной границы - удаляется, и именно эти два ключа
        var aged = await scope.CleanupStaleAsync(DateTime.UtcNow.AddMinutes(1));
        Assert.Equal(new[] { first, second }.OrderBy(k => k), aged.OrderBy(k => k));

        var missing = await Assert.ThrowsAsync<AmazonS3Exception>(() =>
            scope.GetObjectMetadataAsync(first));
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    private static byte[] SmallPng() => Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");

    /// <summary>
    /// Сценарий одного теста: изолированный префикс в бакете и загрузка через presigned POST.
    /// Инфраструктура (настройки, клиент) в <see cref="S3TestEnvironment"/>.
    /// </summary>
    private sealed class SpikeScope : IDisposable
    {
        private string Prefix { get; }
        
        private SpikeScope(S3TestSettings settings, AmazonS3Client s3, string prefix)
        {
            Settings = settings;
            S3 = s3;
            Prefix = prefix;
        }

        public S3TestSettings Settings { get; }

        public AmazonS3Client S3 { get; }

        public static SpikeScope Create()
        {
            var settings = S3TestSettings.FromEnvironment();
            var client = S3TestEnvironment.CreateClient(settings);
            var prefix = $"tests/{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}/";
            return new SpikeScope(settings, client, prefix);
        }

        public string NewKey(string fileName) => Prefix + fileName;

        public Task<CreatePresignedPostResponse> CreatePresignedPostAsync(
            string key,
            string contentType,
            long maxBytes = MaxUploadBytes)
        {
            return S3.CreatePresignedPostAsync(new CreatePresignedPostRequest
            {
                BucketName = Settings.Bucket,
                Key = key,
                Expires = DateTime.UtcNow.AddMinutes(10),
                Fields = new Dictionary<string, string> { ["success_action_status"] = "201" },
                Conditions = new List<S3PostCondition>
                {
                    new ContentLengthRangeCondition(1, maxBytes),
                    new ExactMatchCondition("Content-Type", contentType),
                },
            });
        }

        public async Task<string> GetPreSignedURLAsync(string key)
        {
            return await S3.GetPreSignedURLAsync(new GetPreSignedUrlRequest
            {
                BucketName = Settings.Bucket,
                Key = key,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddMinutes(5),
            });
        }
        
        public async Task<GetObjectMetadataResponse> GetObjectMetadataAsync(string key)
        {
            return await S3.GetObjectMetadataAsync(new GetObjectMetadataRequest
            {
                BucketName = Settings.Bucket,
                Key = key
            });
        }

        public async Task<ListObjectsV2Response> ListObjectsV2Async(int maxKeys=20)
        {
            return await S3.ListObjectsV2Async(new ListObjectsV2Request
            {
                BucketName = Settings.Bucket,
                Prefix = Prefix,
                MaxKeys = maxKeys,
            });
        }

        /// <summary>Загрузка маленького PNG по presigned POST - подготовка данных для теста.</summary>
        public async Task<HttpStatusCode> UploadPngAsync(string key)
        {
            var presigned = await CreatePresignedPostAsync(key, "image/png");
            return await UploadAsync(presigned, SmallPng(), "image/png");
        }

        /// <summary>
        /// Чистка устаревших объектов в пределах префикса запуска. Возвращает удалённые ключи.
        /// </summary>
        public async Task<IReadOnlyList<string>> CleanupStaleAsync(DateTime staleBefore)
        {
            var response = await ListObjectsV2Async(maxKeys: 1000);

            // В SDK v4 коллекции не инициализируются: при пустом результате S3Objects == null
            var stale = (response.S3Objects ?? [])
                .Where(o => (o.LastModified?.ToUniversalTime() ?? DateTime.UtcNow) < staleBefore)
                .Select(o => o.Key)
                .ToList();

            foreach (var key in stale)
                await DeleteAsync(key);

            return stale;
        }

        public async Task<HttpStatusCode> UploadAsync(
            CreatePresignedPostResponse presigned,
            byte[] content,
            string contentType)
        {
            using var http = new HttpClient();
            http.Timeout = TimeSpan.FromMinutes(1);
            using var form = new MultipartFormDataContent();

            foreach (var field in presigned.Fields)
                form.Add(new StringContent(field.Value), field.Key);

            if (!presigned.Fields.ContainsKey("Content-Type"))
                form.Add(new StringContent(contentType), "Content-Type");

            var file = new ByteArrayContent(content);
            file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            form.Add(file, "file", "upload.bin");

            var response = await http.PostAsync(presigned.Url, form);
            return response.StatusCode;
        }

        public Task DeleteAsync(string key) => S3.DeleteObjectAsync(new DeleteObjectRequest
        {
            BucketName = Settings.Bucket,
            Key = key,
        });

        public void Dispose() => S3.Dispose();
    }
}
