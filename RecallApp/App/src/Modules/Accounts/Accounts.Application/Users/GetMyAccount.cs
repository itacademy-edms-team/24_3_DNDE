using Accounts.Application.Abstractions;
using Accounts.Domain;
using ErrorOr;
using MediatR;

namespace Accounts.Application.Users;

/// <summary>Профиль текущего пользователя из доменной строки.</summary>
public sealed record MyAccountResponse(Guid Id, string DisplayName, string Email, bool EmailVerified);

/// <param name="KeycloakSub">Claim sub как есть: разбор и валидация - здесь, чтобы контракт ошибок был один.</param>
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
