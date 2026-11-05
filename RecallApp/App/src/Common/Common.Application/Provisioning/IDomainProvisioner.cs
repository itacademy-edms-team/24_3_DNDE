using System.Security.Claims;

namespace Common.Application.Provisioning;

/// <summary>
/// JIT-провижининг доменных строк модуля из claims аутентифицированного пользователя (ADR 0020).
/// Реализации регистрируются модулями в DI. В Api/ есть общий Middleware,
/// который вызывает все зарегистрированные реализации в модулях.
/// </summary>
public interface IDomainProvisioner
{
    Task EnsureProvisionedAsync(ClaimsPrincipal user, CancellationToken cancellationToken);
}
