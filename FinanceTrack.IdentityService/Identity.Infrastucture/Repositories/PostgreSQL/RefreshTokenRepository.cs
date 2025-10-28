using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.Application.Exceptions;
using Identity.Application.Ports.Repositories;
using Identity.Domain;
using Identity.Infrastucture.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Identity.Infrastucture.Repositories.PostgreSQL
{
    public class RefreshTokenRepository(
        ILogger<RefreshTokenRepository> logger,
        AppIdentityContext appIdentityContext
    ) : IRefreshTokenRepository
    {
        public async Task CreateRefreshTokenAsync(RefreshToken refreshToken)
        {
            try
            {
                await appIdentityContext.RefreshTokens.AddAsync(refreshToken);
                await appIdentityContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(
                    ex,
                    "Failed to create refresh token {TokenId} for user {UserId}",
                    refreshToken.Id,
                    refreshToken.UserId
                );
                throw new RefreshTokenRepositoryException(
                    $"Failed to create refresh token {refreshToken.Id} for user {refreshToken.UserId}",
                    ex
                );
            }
            catch (NpgsqlException ex)
            {
                logger.LogError(
                    ex,
                    "Database error during refresh token {TokenId} creation for user {UserId}",
                    refreshToken.Id,
                    refreshToken.UserId
                );
                throw new RefreshTokenRepositoryException(
                    $"Database error during refresh token {refreshToken.Id} creation for user {refreshToken.UserId}",
                    ex
                );
            }
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(User user)
        {
            var token = await appIdentityContext.RefreshTokens.FirstOrDefaultAsync(t =>
                t.UserId == user.Id
            );
            if (token == null)
            {
                logger.LogWarning("Refresh token not found for user {UserId}", user.Id);
            }
            return token;
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string refreshToken)
        {
            var token = await appIdentityContext.RefreshTokens.FirstOrDefaultAsync(t =>
                t.Token == refreshToken
            );
            if (token == null)
            {
                logger.LogWarning("Refresh token not found in DB");
            }
            return token;
        }

        public async Task UpdateRefreshTokenAsync(RefreshToken refreshToken)
        {
            try
            {
                appIdentityContext.RefreshTokens.Update(refreshToken);
                await appIdentityContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(
                    ex,
                    "Failed to update refresh token with Id {TokenId} for user {UserId}",
                    refreshToken.Id,
                    refreshToken.UserId
                );
                throw new RefreshTokenRepositoryException(
                    $"Failed to update refresh token with Id {refreshToken.Id} for user {refreshToken.UserId}",
                    ex
                );
            }
            catch (NpgsqlException ex)
            {
                logger.LogError(
                    ex,
                    "Database error during refresh token update with Id {TokenId} for user {UserId}",
                    refreshToken.Id,
                    refreshToken.UserId
                );
                throw new RefreshTokenRepositoryException(
                    $"Database error during refresh token update with Id {refreshToken.Id} for user {refreshToken.UserId}",
                    ex
                );
            }
        }

        public async Task RevokeTokenAsync(RefreshToken refreshToken)
        {
            refreshToken.IsRevoked = true;
            await UpdateRefreshTokenAsync(refreshToken);
        }

        public async Task DeleteRefreshTokenAsync(RefreshToken refreshToken)
        {
            try
            {
                appIdentityContext.RefreshTokens.Remove(refreshToken);
                await appIdentityContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(
                    ex,
                    "Failed to delete refresh token {TokenId} for user {UserId}",
                    refreshToken.Id,
                    refreshToken.UserId
                );
                throw new RefreshTokenRepositoryException(
                    $"Failed to delete refresh token {refreshToken.Id} for user {refreshToken.UserId}",
                    ex
                );
            }
            catch (NpgsqlException ex)
            {
                logger.LogError(
                    ex,
                    "Database error during refresh token deletion {TokenId} for user {UserId}",
                    refreshToken.Id,
                    refreshToken.UserId
                );
                throw new RefreshTokenRepositoryException(
                    $"Database error during refresh token {refreshToken.Id} deletion for user {refreshToken.UserId}",
                    ex
                );
            }
        }
    }
}
