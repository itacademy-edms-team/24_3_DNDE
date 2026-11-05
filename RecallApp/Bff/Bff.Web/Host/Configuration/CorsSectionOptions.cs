namespace Host.Configuration;

/// <summary>
/// Секция "Cors" в конфигурации: набор именованных CORS-политик и имя политики по умолчанию.
/// </summary>
public sealed class CorsSectionOptions
{
    public const string SectionName = "Cors";

    /// <summary>
    /// Имя политики по умолчанию (ключ из <see cref="Policies"/>).
    /// Глобальный UseCors() применяет именно её.
    /// </summary>
    public string Default { get; set; } = string.Empty;

    /// <summary>Именованные политики. Ключ словаря - имя политики в конфигурации.</summary>
    public Dictionary<string, CorsPolicyOptions> Policies { get; set; } = new();
}
