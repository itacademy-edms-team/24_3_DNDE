using Identity.Application.Commands.RefreshToken.Request;
using Identity.Application.Commands.RefreshToken.Response;

namespace Identity.Application.Commands.RefreshToken
{
    public interface IRefreshCommand : ICommand<RefreshTokenRequest, RefreshTokenResponse> { }
}
