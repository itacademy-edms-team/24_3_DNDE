namespace Host.Services;

/// <summary>Исход попытки refresh.</summary>
public enum TokenRefreshOutcome
{
    /// <summary>Токены получены.</summary>
    Success,

    /// <summary>Keycloak отказал окончательно (400/401): сессия завершается.</summary>
    Rejected,

    /// <summary>Временный сбой (5xx, таймаут, сеть, битый ответ): сессия сохраняется, повтор после паузы.</summary>
    Unavailable,
}
