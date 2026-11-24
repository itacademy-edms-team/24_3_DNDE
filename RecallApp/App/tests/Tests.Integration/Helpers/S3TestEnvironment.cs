using Amazon;
using Amazon.Runtime;
using Amazon.S3;

namespace Tests.Integration.Helpers;

/// <summary>
/// Настройки S3-совместимого хранилища для интеграционных тестов.
/// Значения берутся из окружения (см. env, секцию Media)
/// </summary>
public sealed record S3TestSettings(
    string Endpoint,
    string Region,
    string Bucket,
    string AccessKeyId,
    string SecretKey)
{
    public static S3TestSettings FromEnvironment()
    {
        static string Require(string name) =>
            Environment.GetEnvironmentVariable(name) is { Length: > 0 } value
                ? value
                : throw new InvalidOperationException(
                    $"Не задана переменная {name}: тесты хранилища требуют реального доступа к cloud.ru (ADR 0037)");

        return new S3TestSettings(
            Require("Media__S3__Endpoint"),
            Require("Media__S3__Region"),
            Require("Media__S3__Bucket"),
            Require("Media__S3__AccessKeyId"),
            Require("Media__S3__SecretKey"));
    }
}

/// <summary>
/// Фабрика S3-клиента для тестов. Единственное место, где настраивается endpoint cloud.ru:
/// порядок важен, в AWS SDK v4 <c>RegionEndpoint</c> перекрывает <c>ServiceURL</c> (spike, ADR 0037).
/// Virtual-hosted (без ForcePathStyle) - как в проде: cloud.ru применяет CORS только к virtual-hosted URL.
/// </summary>
public static class S3TestEnvironment
{
    public static AmazonS3Client CreateClient(S3TestSettings settings)
    {
        var config = new AmazonS3Config
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(settings.Region),
            AuthenticationRegion = settings.Region,
        };
        config.ServiceURL = settings.Endpoint;

        return new AmazonS3Client(new BasicAWSCredentials(settings.AccessKeyId, settings.SecretKey), config);
    }
}
