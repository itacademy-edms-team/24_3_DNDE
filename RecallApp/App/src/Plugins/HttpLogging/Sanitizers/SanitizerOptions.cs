namespace HttpLogging.Sanitizers;

/// <summary>
/// Параметры маскирования сенситивных данных.
/// </summary>
public sealed class SanitizerOptions
{
    /// <summary>
    /// Подвергаемые маскировке ключи/поля.
    /// </summary>
    public HashSet<string> SensitiveKeys { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "password", "passwd", "secret", "token", "access_token", "refresh_token",
        "authorization", "cookie", "set-cookie", "api_key", "apikey"
    };

    /// <summary>
    /// Значение для подмены (маскирования) исходного значения.
    /// </summary>
    public string MaskValue { get; set; } = "[MASKED]";
}
