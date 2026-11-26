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

    /// <summary>
    /// Добавляет ключ в набор маскируемых полей.
    /// </summary>
    /// <param name="key">Имя ключа/поля.</param>
    /// <returns>Текущий экземпляр для цепочки вызовов.</returns>
    public SanitizerOptions AddSensitiveKey(string key)
    {
        SensitiveKeys.Add(key);
        return this;
    }
    /// <summary>
    /// Добавляет ключи в набор маскируемых полей.
    /// </summary>
    /// <param name="keys">Имена ключей/полей.</param>
    /// <returns>Текущий экземпляр для цепочки вызовов.</returns>
    public SanitizerOptions AddSensitiveKey(params string[] keys)
    {
        foreach (var key in keys)
            AddSensitiveKey(key);
        return this;
    }

    /// <summary>
    /// Удаляет ключ из набора маскируемых полей.
    /// </summary>
    /// <param name="key">Имя ключа/поля.</param>
    /// <returns>Текущий экземпляр для цепочки вызовов.</returns>
    public SanitizerOptions RemoveSensitiveKey(string key)
    {
        SensitiveKeys.Remove(key);
        return this;
    }

    /// <summary>
    /// Удаляет ключи из набора маскируемых полей.
    /// </summary>
    /// <param name="keys">Имена ключей/полей</param>
    /// <returns>Текущий экземпляр для цепочки вызовов.</returns>
    public SanitizerOptions RemoveSensitiveKey(params string[] keys)
    {
        foreach (var key in keys)
            RemoveSensitiveKey(key);
        return this;
    }
}
