namespace Media.Infrastructure.Storage;

/// <summary>
/// Настройки объектного хранилища из секции `Media:S3`.
/// </summary>
internal sealed class S3Options
{
    public const string SectionName = "Media:S3";

    public string Endpoint { get; set; } = string.Empty;

    public string Region { get; set; } = string.Empty;

    public string Bucket { get; set; } = string.Empty;

    public string AccessKeyId { get; set; } = string.Empty;

    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Возвращает имена незаполненных обязательных параметров.</summary>
    public IReadOnlyList<string> Missing()
    {
        var missing = new List<string>(5);

        if (string.IsNullOrWhiteSpace(Endpoint)) 
            missing.Add(nameof(Endpoint));
        if (string.IsNullOrWhiteSpace(Region)) 
            missing.Add(nameof(Region));
        if (string.IsNullOrWhiteSpace(Bucket)) 
            missing.Add(nameof(Bucket));
        if (string.IsNullOrWhiteSpace(AccessKeyId)) 
            missing.Add(nameof(AccessKeyId));
        if (string.IsNullOrWhiteSpace(SecretKey)) 
            missing.Add(nameof(SecretKey));

        return missing;
    }
}
