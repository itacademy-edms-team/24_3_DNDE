using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using ErrorOr;
using Media.Application.Storage;
using Media.Domain.Objects;

namespace Media.Infrastructure.Storage;

/// <summary>
/// Реализует объектное хранилище поверх S3 (cloud.ru).
/// </summary>
internal sealed class S3ObjectStorage : IObjectStorage
{
    private const string WebpContentType = "image/webp";

    private readonly IAmazonS3 _s3;

    private string Bucket { get; }
    
    internal S3ObjectStorage(IAmazonS3 s3, string bucket)
    {
        _s3 = s3;
        Bucket = bucket;
    }

    public async Task<ErrorOr<UploadPolicy>> CreateUploadPolicyAsync(
        string objectKey,
        string contentType,
        long maxBytes,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var expires = now + MediaConstraints.UploadWindow;

        var request = new CreatePresignedPostRequest
        {
            BucketName = Bucket,
            Key = objectKey,
            Expires = expires,
            // В AWSSDK v4 коллекции по умолчанию null: инициализируем явно.
            // `Content-Type` - поле формы: политика проверяет его, а не заголовок файла.
            Fields = new Dictionary<string, string>
            {
                ["Content-Type"] = contentType,
                ["success_action_status"] = "201",
            },
            Conditions = new List<S3PostCondition>
            {
                new ExactMatchCondition("Content-Type", contentType),
                new ContentLengthRangeCondition(MediaConstraints.MinUploadBytes, maxBytes),
            },
        };

        var response = await _s3.CreatePresignedPostAsync(request);

        return new UploadPolicy(
            response.Url,
            new Dictionary<string, string>(response.Fields),
            objectKey,
            expires);
    }

    public async Task<StoredObjectInfo?> HeadAsync(string objectKey, CancellationToken cancellationToken)
    {
        try
        {
            var metadata = await _s3.GetObjectMetadataAsync(
                new GetObjectMetadataRequest { BucketName = Bucket, Key = objectKey },
                cancellationToken);

            return new StoredObjectInfo(metadata.ContentLength, metadata.Headers.ContentType, metadata.LastModified);
        }
        catch (AmazonS3Exception exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<ErrorOr<string>> CreateReadUrlAsync(string objectKey, TimeSpan lifetime, CancellationToken cancellationToken)
    {
        var url = await _s3.GetPreSignedURLAsync(new GetPreSignedUrlRequest
        {
            BucketName = Bucket,
            Key = objectKey,
            Expires = DateTime.UtcNow + lifetime,
            Verb = HttpVerb.GET,
        });

        return string.IsNullOrWhiteSpace(url)
            ? Media.Domain.Objects.MediaErrors.ObjectMissing
            : url;
    }

    public async Task<ErrorOr<byte[]>> ReadAsync(string objectKey, long maxBytes, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _s3.GetObjectAsync(
                new GetObjectRequest { BucketName = Bucket, Key = objectKey },
                cancellationToken);

            // Размер проверяем до вычитывания: не тянем в память то, что всё равно отклоним.
            if (response.ContentLength > maxBytes)
                return MediaErrors.FileTooLarge;

            using var buffer = new MemoryStream();
            await response.ResponseStream.CopyToAsync(buffer, cancellationToken);
            return buffer.ToArray();
        }
        catch (AmazonS3Exception exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return MediaErrors.ObjectMissing;
        }
    }

    public async Task<ErrorOr<Success>> WriteAsync(
        string objectKey,
        byte[] content,
        string contentType,
        CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream(content, writable: false);

        await _s3.PutObjectAsync(new PutObjectRequest
        {
            BucketName = Bucket,
            Key = objectKey,
            InputStream = stream,
            ContentType = string.IsNullOrWhiteSpace(contentType) ? WebpContentType : contentType,
            AutoCloseStream = false,
        }, cancellationToken);

        return Result.Success;
    }

    public async Task<ErrorOr<Success>> DeleteAsync(string objectKey, CancellationToken cancellationToken)
    {
        // DeleteObject идемпотентен: отсутствие объекта - не ошибка.
        await _s3.DeleteObjectAsync(new DeleteObjectRequest { BucketName = Bucket, Key = objectKey }, cancellationToken);
        return Result.Success;
    }

    /// <summary>
    /// Создаёт клиент объектного хранилища.
    /// </summary>
    /// <remarks>
    /// <para>Сначала задаётся регион, потом ServiceURL - иначе SDK уходит на amazonaws.com вместо
    /// cloud.ru (проверено spike-тестом, ADR 0037).</para>
    /// <para>Virtual-hosted (без ForcePathStyle) обязателен: cloud.ru применяет CORS только к URL
    /// вида <c>{bucket}.s3.cloud.ru</c>, а path-style обходит CORS-движок, и префлайт получает 403.</para>
    /// </remarks>
    /// <param name="options">Настройки из секции <c>Media:S3</c>.</param>
    /// <returns>Клиент S3 с заданным регионом и ServiceURL.</returns>
    internal static AmazonS3Client CreateClient(S3Options options)
    {
        var config = new AmazonS3Config
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(options.Region),
            AuthenticationRegion = options.Region,
        };
        config.ServiceURL = options.Endpoint;

        return new AmazonS3Client(new BasicAWSCredentials(options.AccessKeyId, options.SecretKey), config);
    }
}
