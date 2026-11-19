namespace Media.Infrastructure.Storage;

/// <summary>
/// Содержит настройки объектного хранилища (секция <c>Media:S3</c>).
/// </summary>
internal sealed class S3Options
{
    /// <summary>Имя секции конфигурации.</summary>
    public const string SectionName = "Media:S3";

    /// <summary>Адрес объектного хранилища.</summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>Регион хранилища.</summary>
    public string Region { get; set; } = string.Empty;

    /// <summary>Имя бакета.</summary>
    public string Bucket { get; set; } = string.Empty;

    /// <summary>Идентификатор ключа доступа.</summary>
    public string AccessKeyId { get; set; } = string.Empty;

    /// <summary>Секретный ключ доступа.</summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Возвращает имена незаполненных обязательных параметров.
    /// </summary>
    /// <returns>Имена свойств с пустым значением; пустой список, если заданы все.</returns>
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
