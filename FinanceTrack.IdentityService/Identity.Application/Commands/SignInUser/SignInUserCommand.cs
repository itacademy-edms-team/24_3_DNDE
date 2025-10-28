using Identity.Application.Commands.SignInUser.Request;
using Identity.Application.Commands.SignInUser.Response;
using Identity.Application.Exceptions;
using Identity.Application.Ports.Services;
using Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Commands.SignInUser
{
    public class SignInUserCommand(
        ILogger<SignInUserCommand> logger,
        UserManager<User> userManager,
        IUserPasswordSignInService passwordSignInService,
        IAuthTokenService authTokenService
    ) : ISignInUserCommand
    {
        public async Task<SignInUserResponse> Execute(SignInUserRequest request)
        {
            try
            {
                var user = await userManager.FindByEmailAsync(request.Email);

                // User not exist
                if (user == null)
                {
                    logger.LogWarning("Sign-in attempt with invalid email: {Email}", request.Email);
                    return new SignInUserResponse
                    {
                        IsSuccess = false,
                        Error = new ProblemDetails
                        {
                            Status = 401,
                            Title = "Unauthorized",
                            Detail = "Invalid email or password",
                        },
                    };
                }

                var signInResult = await passwordSignInService.CheckPassword(
                    user,
                    request.Password
                );

                // Invalid password
                if (!signInResult.Succeeded)
                {
                    logger.LogWarning(
                        "Sign-in attempt with invalid password: {Email}",
                        request.Email
                    );
                    return new SignInUserResponse
                    {
                        IsSuccess = false,
                        Error = new ProblemDetails
                        {
                            Status = 401,
                            Title = "Unauthorized",
                            Detail = "Invalid email or password",
                        },
                    };
                }

                // Generating tokens
                var accessTokenString = await authTokenService.GenerateAccessTokenAsync(user);
                var refreshTokenString = await authTokenService.GenerateRefreshTokenAsync(user);

                return new SignInUserResponse
                {
                    IsSuccess = true,
                    AccessToken = accessTokenString,
                    RefreshToken = refreshTokenString,
                };
            }
            catch (RefreshTokenRepositoryException ex)
            {
                logger.LogError(ex, "Database error during sign-in for {Email}", request.Email);
                return new SignInUserResponse
                {
                    IsSuccess = false,
                    Error = new ProblemDetails
                    {
                        Status = 500,
                        Title = "Server Error",
                        Detail = "Database error occurred",
                    },
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error during sign-in for {Email}", request.Email);
                return new SignInUserResponse
                {
                    IsSuccess = false,
                    Error = new ProblemDetails
                    {
                        Status = 500,
                        Title = "Server Error",
                        Detail = "An unexpected error occurred",
                    },
                };
            }
        }
    }
}
