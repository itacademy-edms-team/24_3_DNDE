using System.Security.Claims;

namespace Common.Application.Provisioning;

/// <summary>
/// JIT-провижининг доменных строк модуля из claims аутентифицированного пользователя (ADR 0020).
/// Реализации регистрируются модулями в DI. В Api/ есть общий Middleware,
/// который вызывает все зарегистрированные реализации в модулях.
/// </summary>
public interface IDomainProvisioner
{
    /// <summary>
    /// Гарантирует существование доменных строк модуля для текущей личности.
    /// </summary>
    /// <param name="user">Аутентифицированный пользователь (claims из токена).</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>
    /// Id доменного пользователя, если модуль владеет строками пользователей (Accounts);
    /// null, если модуль провижинит только собственные сущности (Media и т.п.).
    /// Первый ненулевой результат кладётся в <see cref="ICurrentUser"/>.
    /// </returns>
    Task<Guid?> EnsureProvisionedAsync(ClaimsPrincipal user, CancellationToken cancellationToken);
}
