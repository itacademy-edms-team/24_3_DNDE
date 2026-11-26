namespace HttpLogging;

/// <summary>
/// Настройки захвата тел HTTP-запроса и ответа.
/// </summary>
public sealed class BodyCaptureOptions
{
    /// <summary>
    /// Возвращает или задает признак захвата тела запроса.
    /// </summary>
    public bool CaptureRequestBody { get; set; } = true;

    /// <summary>
    /// Возвращает или задает признак захвата тела ответа.
    /// </summary>
    public bool CaptureResponseBody { get; set; } = true;

    /// <summary>
    /// Возвращает или задает максимальную длину сохраняемого тела в символах (0 - без ограничения).
    /// </summary>
    public int MaxLength { get; set; }

    /// <summary>
    /// Возвращает префиксы content-type, тела которых захватываются.
    /// </summary>
    /// <remarks>
    /// Сравнение по префиксу без учёта регистра (например, "application/json").
    /// </remarks>
    public HashSet<string> ContentTypes { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/json",
        "application/problem+json",
        "application/x-www-form-urlencoded",
        "application/xml",
        "text/",
    };

    /// <summary>
    /// Добавляет новый захватываемый тип содержимого в набор.
    /// </summary>
    /// <param name="contentType">Тип содержимого.</param>
    /// <returns>Тот же экземпляр для цепочки вызовов.</returns>
    public BodyCaptureOptions AddContentType(string contentType)
    {
        ContentTypes.Add(contentType);
        return this;
    }
    
    /// <summary>
    /// Добавляет новые захватываемые типы содержимого в набор.
    /// </summary>
    /// <param name="contentTypes">Типы содержимого.</param>
    /// <returns>Тот же экземпляр для цепочки вызовов.</returns>
    public BodyCaptureOptions AddContentType(params string[] contentTypes)
    {
        foreach(var contentType in ContentTypes)
            AddContentType(contentType);
        return this;
    }

    /// <summary>
    /// Убирает захватываемый тип содержимого из набора. 
    /// </summary>
    /// <param name="contentType">Тип содержимого.</param>
    /// <returns>Тот же экземпляр для цепочки вызовов.</returns>
    public BodyCaptureOptions RemoveContentType(string contentType)
    {
        ContentTypes.Remove(contentType);
        return this;
    }
    
    /// <summary>
    /// Убирает захватываемые типы содержимого из набора. 
    /// </summary>
    /// <param name="contentTypes">Типы содержимого.</param>
    /// <returns>Тот же экземпляр для цепочки вызовов.</returns>
    public BodyCaptureOptions RemoveContentType(params string[] contentTypes)
    {
        foreach(var contentType in ContentTypes)
            RemoveContentType(contentType);
        return this;
    }
}
