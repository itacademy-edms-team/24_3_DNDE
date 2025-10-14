using Identity.Application.UseCases.RefreshToken.Request;
using Identity.Application.UseCases.RefreshToken.Response;

namespace Identity.Application.UseCases.RefreshToken
{
    public interface IRefreshTokenUseCase : IUseCase<RefreshTokenRequest, RefreshTokenResponse> { }
}
