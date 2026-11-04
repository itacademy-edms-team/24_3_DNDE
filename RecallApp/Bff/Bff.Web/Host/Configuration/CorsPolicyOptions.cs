namespace Host.Configuration;

/// <summary>
/// Одна именованная CORS-политика (элемент <see cref="CorsSectionOptions.Policies"/>).
/// Свойства повторяют CorsPolicyBuilder.
/// </summary>
public sealed class CorsPolicyOptions
{
    /// <summary>
    /// Передавать ли cookie. Нужно для сессии BFF.
    /// Требует явного списка origin'ов: с Any браузер ответ отбрасывает.
    /// </summary>
    public bool AllowCredentials { get; set; }

    /// <summary>Разрешённые origin'ы браузерных клиентов.</summary>
    public CorsValueSet AllowedOrigins { get; set; } = new();

    /// <summary>Разрешённые заголовки запроса.</summary>
    public CorsValueSet AllowedHeaders { get; set; } = new() { Any = true };

    /// <summary>Разрешённые методы запроса.</summary>
    public CorsValueSet AllowedMethods { get; set; } = new() { Any = true };

    /// <summary>Заголовки ответа, доступные для чтения из JavaScript.</summary>
    public string[] ExposedHeaders { get; set; } = [];

    /// <summary>
    /// Время кэширования preflight-ответа браузером.
    /// </summary>
    public TimeSpan? PreflightMaxAge { get; set; }
}
