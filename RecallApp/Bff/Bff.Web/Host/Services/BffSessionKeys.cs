namespace Host.Services;

/// <summary>
/// Ключи BFF-сессии в <see cref="Microsoft.AspNetCore.Authentication.AuthenticationProperties"/>.
/// </summary>
public static class BffSessionKeys
{
    /// <summary>
    /// Стабильный идентификатор BFF-сессии (session-over-session):
    /// ставится один раз при входе, переживает refresh (properties сохраняются), меняется только с новым логином.
    /// Служит ключом сериализации обновления токенов - одна цепочка токенов = один BFF-тикет.
    /// </summary>
    public const string SessionId = "bff_session_id";
}
