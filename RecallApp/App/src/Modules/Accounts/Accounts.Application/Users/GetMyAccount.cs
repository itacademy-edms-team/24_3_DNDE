using Accounts.Application.Abstractions;
using MediatR;

namespace Accounts.Application.Users;

/// <summary>Профиль текущего пользователя из доменной строки.</summary>
public sealed record MyAccountResponse(Guid Id, string DisplayName, string Email, bool EmailVerified);

public sealed record GetMyAccountQuery(Guid KeycloakSub, bool EmailVerified) : IRequest<MyAccountResponse?>;

public sealed class GetMyAccountHandler(IUserRepository users) : IRequestHandler<GetMyAccountQuery, MyAccountResponse?>
{
    public async Task<MyAccountResponse?> Handle(GetMyAccountQuery query, CancellationToken cancellationToken)
    {
        var user = await users.FindByKeycloakSubAsync(query.KeycloakSub, cancellationToken);
        return user is null
            ? null
            : new MyAccountResponse(user.Id, user.DisplayName, user.Email, query.EmailVerified);
    }
}
