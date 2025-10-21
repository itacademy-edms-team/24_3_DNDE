using Identity.Application.Commands.RefreshToken;
using Identity.Application.Commands.RefreshToken.Request;
using Identity.Application.Commands.RefreshToken.Response;
using Identity.Application.Enums;
using Identity.Application.Exceptions;
using Identity.Application.Ports.Repositories;
using Identity.Application.Ports.Services;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Commands.RefreshToken
{
    public class RefreshTokenCommand(
        ILogger<RefreshTokenCommand> logger,
        IAuthTokenService authTokenService,
        IIdentityRepository repo
    ) : IRefreshCommand
    {
        public async Task<RefreshTokenResponse> Execute(RefreshTokenRequest request)
        {
            try
            {
                if (!await authTokenService.IsTokenValid(request.AccessToken, false))
                {
                    return new RefreshTokenErrorResponse
                    {
                        Message = Enum.GetName(ErrorCodes.AccessTokenIsNotValid),
                        Code = ErrorCodes.AccessTokenIsNotValid.ToString("D"),
                    };
                }

                var userId = await authTokenService.GetUserIdFromToken(request.AccessToken);
                var user = await repo.GetUserByUserId(userId);

                if (!user.RefreshToken.Active)
                {
                    return new RefreshTokenErrorResponse
                    {
                        Message = Enum.GetName(ErrorCodes.RefreshTokenIsNotActive),
                        Code = ErrorCodes.RefreshTokenIsNotActive.ToString("D"),
                    };
                }

                if (user.RefreshToken.ExpirationDate < DateTime.UtcNow)
                {
                    return new RefreshTokenErrorResponse
                    {
                        Message = Enum.GetName(ErrorCodes.RefreshTokenHasExpired),
                        Code = ErrorCodes.RefreshTokenHasExpired.ToString("D"),
                    };
                }

                if (user.RefreshToken.Value != request.RefreshToken)
                {
                    return new RefreshTokenErrorResponse
                    {
                        Message = Enum.GetName(ErrorCodes.RefreshTokenIsNotCorrect),
                        Code = ErrorCodes.RefreshTokenIsNotCorrect.ToString("D"),
                    };
                }

                var newToken = await authTokenService.GenerateAccessToken(user);

                user.RefreshToken.Value = await authTokenService.GenerateRefreshToken();
                user.RefreshToken.Active = true;
                user.RefreshToken.ExpirationDate = DateTime.UtcNow.AddMinutes(
                    await authTokenService.GetRefreshTokenLifetimeInMinutes()
                );
                await repo.UpdateUser(user);

                return new RefreshTokenSuccessResponse
                {
                    AccessToken = newToken,
                    RefreshToken = user.RefreshToken.Value,
                    RefreshTokenExpirationDate = user.RefreshToken.ExpirationDate,
                };
            }
            catch (InvalidTokenException ex)
            {
                logger.LogError(ex, ex.Message);
                return new RefreshTokenErrorResponse
                {
                    Message = Enum.GetName(ErrorCodes.AccessTokenIsNotValid),
                    Code = ErrorCodes.AccessTokenIsNotValid.ToString("D"),
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return new RefreshTokenErrorResponse
                {
                    Message = Enum.GetName(ErrorCodes.AnUnexpectedErrorOcurred),
                    Code = ErrorCodes.AnUnexpectedErrorOcurred.ToString("D"),
                };
            }
        }
    }
}
