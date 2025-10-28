using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.Application.Commands.RefreshTokens.Request;
using Identity.Application.Commands.RefreshTokens.Response;
using Identity.Application.Exceptions;
using Identity.Application.Ports.Repositories;
using Identity.Application.Ports.Services;
using Identity.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Commands.RefreshTokens
{
    public class RefreshTokensCommand(
        ILogger<RefreshTokensCommand> logger,
        IRefreshTokenRepository refreshTokenRepository,
        UserManager<User> userManager,
        IAuthTokenService authTokenService
    ) : IRefreshTokensCommand
    {
        public async Task<RefreshTokensResponse> Execute(RefreshTokensRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.RefreshToken))
                {
                    return new RefreshTokensResponse
                    {
                        IsSuccess = false,
                        Error = new ProblemDetails
                        {
                            Status = 401,
                            Title = "Unauthorized",
                            Detail = "Refresh token is not provided",
                        },
                    };
                }

                // Get refreshToken from db
                var refreshTokenModel = await refreshTokenRepository.GetRefreshTokenAsync(
                    request.RefreshToken
                );

                // Provided refresh token is not in store
                if (refreshTokenModel == null)
                {
                    logger.LogWarning(
                        "Refresh token not found in DB {Token}",
                        request.RefreshToken
                    );
                    return new RefreshTokensResponse
                    {
                        IsSuccess = false,
                        Error = new ProblemDetails
                        {
                            Status = 401,
                            Title = "Unauthorized",
                            Detail = "Refresh token is invalid",
                        },
                    };
                }

                // Check token validity: not revoked, not expired
                var dtUtcNow = DateTime.UtcNow;
                if (refreshTokenModel.IsRevoked || refreshTokenModel.Expires <= dtUtcNow)
                {
                    if (refreshTokenModel.IsRevoked)
                    {
                        logger.LogWarning("Provided revoked token {TokenId}", refreshTokenModel.Id);
                    }
                    else
                    {
                        logger.LogWarning("Provided expired token {TokenId}", refreshTokenModel.Id);
                    }
                    return new RefreshTokensResponse
                    {
                        IsSuccess = false,
                        Error = new ProblemDetails
                        {
                            Status = 401,
                            Title = "Unauthorized",
                            Detail = "Refresh token is invalid",
                        },
                    };
                }

                // Get User of this token
                var user = await userManager.FindByIdAsync(refreshTokenModel.UserId.ToString());
                // User not found
                if (user == null)
                {
                    logger.LogWarning(
                        "Refresh tokens attempt for non-existing user {UserId} (token {TokenId})",
                        refreshTokenModel.UserId,
                        refreshTokenModel.Id
                    );
                    return new RefreshTokensResponse
                    {
                        IsSuccess = false,
                        Error = new ProblemDetails
                        {
                            Status = 401,
                            Title = "Unauthorized",
                            Detail = "User not found",
                        },
                    };
                }

                // Generate new token pair
                var newAccessToken = await authTokenService.GenerateAccessTokenAsync(user);
                var newRefreshToken = await authTokenService.GenerateRefreshTokenAsync(user);

                // Perform token rotation (Revoke old token)
                await refreshTokenRepository.RevokeTokenAsync(refreshTokenModel);

                return new RefreshTokensResponse
                {
                    IsSuccess = true,
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                };
            }
            catch (RefreshTokenRepositoryException ex)
            {
                logger.LogError(ex, "DB error during refresh");
                return new RefreshTokensResponse
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
                logger.LogError(ex, "Unexpected error during refresh");
                return new RefreshTokensResponse
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
