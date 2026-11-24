namespace Api;

/// <summary>
/// Предоставляет значение источника ошибки в problem-ответах.
/// </summary>
/// <remarks>
/// Поле <c>source</c> отличает ошибки API от ошибок BFF для клиента и логов.
/// </remarks>
public static class ProblemSource
{
    /// <summary>Источник проблемы, порождённой API-приложением.</summary>
    public const string App = "app";
}
