namespace Common.Application.Provisioning;

/// <summary>
/// Определяет идентичность доменного пользователя в рамках текущего запроса.
/// </summary>
/// <remarks>
/// Заполняется ступенью JIT-провижининга (Api/DomainProvisioning):
/// модули не ходят в чужие схемы за идентификатором пользователя (ADR 0036).
/// </remarks>
public interface ICurrentUser
{
    /// <summary>
    /// Возвращает идентификатор доменного пользователя; <see langword="null"/>, если запрос
    /// не аутентифицирован или личность не сопоставлена доменной строке.
    /// </summary>
    Guid? UserId { get; }
}
