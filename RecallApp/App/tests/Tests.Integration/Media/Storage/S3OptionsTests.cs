using Media.Infrastructure.Storage;

namespace Tests.Integration.Media.Storage;

/// <summary>
/// Настройки хранилища проверяются на старте: неполная конфигурация должна валить запуск,
/// а не первый запрос с файлом (онтология Media §6.8).
/// </summary>
public sealed class S3OptionsTests
{
    [Fact]
    public void Missing_EmptyOptions_ReturnsAllRequiredNames()
    {
        var options = new S3Options();

        var missing = options.Missing();

        Assert.Equal(
            ["Endpoint", "Region", "Bucket", "AccessKeyId", "SecretKey"],
            missing);
    }

    [Fact]
    public void Missing_PartiallyFilled_ReturnsOnlyEmptyNames()
    {
        var options = new S3Options
        {
            Endpoint = "https://s3.cloud.ru",
            Region = "ru-central-1",
            Bucket = "recallapp-dev",
        };

        var missing = options.Missing();

        Assert.Equal(["AccessKeyId", "SecretKey"], missing);
    }

    [Fact]
    public void Missing_WhitespaceValues_CountsAsEmpty()
    {
        var options = new S3Options
        {
            Endpoint = "https://s3.cloud.ru",
            Region = "  ",
            Bucket = "recallapp-dev",
            AccessKeyId = "tenant:key",
            SecretKey = "secret",
        };

        Assert.Equal(["Region"], options.Missing());
    }

    [Fact]
    public void Missing_FullyConfigured_ReturnsNothing()
    {
        var options = new S3Options
        {
            Endpoint = "https://s3.cloud.ru",
            Region = "ru-central-1",
            Bucket = "recallapp-dev",
            AccessKeyId = "tenant:key",
            SecretKey = "secret",
        };

        Assert.Empty(options.Missing());
    }
}
