namespace Host.Services;

/// <summary>
/// Ключи <see cref="Microsoft.AspNetCore.Http.HttpContext.Items"/> конвейера BFF:
/// состояние запроса, которым обмениваются компоненты внутри одного запроса.
/// </summary>
public static class BffRequestItems
{
    /// <summary>
    /// Маркер отказа Keycloak на обновление токена в этом запросе.
    /// Ставится <see cref="TokenRefreshService"/> вместе с sign-out серверной сессии;
    /// читает SessionGuardMiddleware, чтобы ответить 401 на /api/** и сбросить личность на остальных путях.
    /// </summary>
    public const string SessionRejected = "bff_session_rejected";
}
