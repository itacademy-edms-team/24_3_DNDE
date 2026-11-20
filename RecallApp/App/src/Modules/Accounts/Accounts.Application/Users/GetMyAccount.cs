using Accounts.Application.Abstractions;
using Accounts.Domain;
using ErrorOr;
using MediatR;

namespace Accounts.Application.Users;

/// <summary>
/// Представляет профиль текущего пользователя.
/// </summary>
/// <param name="Id">Идентификатор пользователя в приложении.</param>
/// <param name="DisplayName">Отображаемое имя из домена.</param>
/// <param name="Email">Email из доменного кэша.</param>
/// <param name="EmailVerified">Подтверждён ли email провайдером личности.</param>
public sealed record MyAccountResponse(Guid Id, string DisplayName, string Email, bool EmailVerified);

/// <summary>
/// Возвращает профиль текущего пользователя по claims личности.
/// </summary>
/// <param name="KeycloakSub">Claim <c>sub</c> как есть: разбор и валидация выполняются здесь,
/// чтобы невалидный идентификатор всегда давал один код <c>User.KeycloakSubInvalid</c>,
/// а не ошибку биндинга из HTTP-слоя.</param>
/// <param name="EmailVerified">Claim <c>email_verified</c>,
/// проставляется в ответ без проверки в домене.</param>
public sealed record GetMyAccountQuery(string? KeycloakSub, bool EmailVerified)
    : IRequest<ErrorOr<MyAccountResponse>>;

public sealed class GetMyAccountHandler(IUserRepository users)
    : IRequestHandler<GetMyAccountQuery, ErrorOr<MyAccountResponse>>
{
    public async Task<ErrorOr<MyAccountResponse>> Handle(
        GetMyAccountQuery query,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(query.KeycloakSub, out var keycloakSub))
            return UserErrors.KeycloakSubInvalid;

        var user = await users.FindByKeycloakSubAsync(keycloakSub, cancellationToken);
        return user is null
            ? UserErrors.NotFound
            : new MyAccountResponse(user.Id, user.DisplayName, user.Email, query.EmailVerified);
    }
}
