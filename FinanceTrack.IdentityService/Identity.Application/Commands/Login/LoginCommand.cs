using Identity.Application.Commands.Login.Request;
using Identity.Application.Commands.Login.Response;
using Identity.Application.Enums;
using Identity.Application.Exceptions;
using Identity.Application.Ports.Repositories;
using Identity.Application.Ports.Services;
using Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Commands.Login
{
    public class LoginCommand(
        ILogger<LoginCommand> logger,
        IIdentityRepository authRepo,
        ITokenRepository tokenRepo,
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IAuthTokenService jwtService
    ) : ILoginCommand
    {
        public async Task<LoginResponse> Execute(LoginRequest request)
        {
            try
            {
                var user = await authRepo.GetUserByEmail(request.Email);

                if (user == null)
                {
                    throw new LoginUserException("User not found");
                }

                var signInResult = await signInManager.CheckPasswordSignInAsync(
                    user,
                    request.Password,
                    false
                );
                if (!signInResult.Succeeded)
                {
                    return new LoginErrorResponse
                    {
                        Message = Enum.GetName(ErrorCodes.CredentialsAreNotValid),
                        Code = ErrorCodes.CredentialsAreNotValid.ToString("D"),
                    };
                }

                // Tokens
                var accessToken = await jwtService.GenerateAccessToken(user);
                var refreshToken = await jwtService.GenerateRefreshToken(user);
                tokenRepo.CreateRefreshToken(user.Id, refreshToken);

                return new LoginSuccessResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                };
            }
            catch (LoginUserException ex)
            {
                logger.LogWarning(ex, "Login failed for email: {Email}", request.Email);
                return new LoginErrorResponse
                {
                    Message = ex.Message,
                    Code = ErrorCodes.UserLoginFailed.ToString("D"),
                };
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Unexpected error during login for email: {Email}",
                    request.Email
                );
                return new LoginErrorResponse
                {
                    Message = Enum.GetName(ErrorCodes.AnUnexpectedErrorOcurred),
                    Code = ErrorCodes.AnUnexpectedErrorOcurred.ToString("D"),
                };
            }
        }
    }
}
