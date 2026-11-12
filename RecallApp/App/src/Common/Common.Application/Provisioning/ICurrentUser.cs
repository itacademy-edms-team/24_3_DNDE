namespace Common.Application.Provisioning;

/// <summary>
/// Идентичность доменного пользователя в рамках текущего запроса.
/// Заполняется ступенью JIT-провижининга (Api/DomainProvisioning):
/// модули не ходят в чужие схемы за id пользователя (ADR 0036).
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Id доменного пользователя или null,
    /// если запрос не аутентифицирован или личность не удалось сопоставить с доменной строкой.
    /// </summary>
    Guid? UserId { get; }
}
