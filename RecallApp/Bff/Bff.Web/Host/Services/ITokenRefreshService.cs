namespace Host.Services;

/// <summary>Упреждающее обновление access-токена сессии текущего запроса.</summary>
public interface ITokenRefreshService
{
    /// <summary>
    /// Обновляет токены сессии, если до истечения access-токена осталось меньше порога.
    /// Окончательный отказ Keycloak завершает сессию; временный сбой откладывает повтор.
    /// Для анонимного запроса ничего не делает.
    /// </summary>
    Task RefreshIfNeededAsync(HttpContext context);
}
