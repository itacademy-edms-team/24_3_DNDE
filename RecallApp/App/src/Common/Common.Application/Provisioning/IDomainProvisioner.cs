using System.Security.Claims;

namespace Common.Application.Provisioning;

/// <summary>
/// Определяет провижининг доменных строк модуля из claims аутентифицированного пользователя.
/// </summary>
/// <remarks>
/// <para>Реализации регистрируются модулями в DI.</para>
/// <para>Общий Middleware в Api вызывает все зарегистрированные реализации;
/// первый ненулевой результат кладётся в <see cref="ICurrentUser"/> (ADR 0020).</para>
/// </remarks>
public interface IDomainProvisioner
{
    /// <summary>
    /// Гарантирует существование доменных строк модуля для текущей личности.
    /// </summary>
    /// <param name="user">Аутентифицированный пользователь (claims из токена).</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>
    /// Идентификатор доменного пользователя, если модуль владеет строками пользователей (Accounts);
    /// <see langword="null"/>, если модуль провижинит только собственные сущности (Media и т.п.).
    /// </returns>
    Task<Guid?> EnsureProvisionedAsync(ClaimsPrincipal user, CancellationToken cancellationToken);
}
