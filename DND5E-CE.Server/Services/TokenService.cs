using DND5E_CE.Server.Data;
using DND5E_CE.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Isopoh.Cryptography.Argon2; // Добавляем библиотеку Argon2

namespace DND5E_CE.Server.Services
{
    public interface ITokenService
    {
        Task<TokenResponse> GenerateTokensAsync(IdentityUser user);
        Task<TokenResponse> RefreshTokensAsync(string refreshToken);
        Task<bool> RevokeTokenAsync(string refreshToken, string userId);
        Task<bool> RevokeAllTokensAsync(string userId);
    }

    public class TokenService : ITokenService
    {
        private readonly DND5EContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TokenService> _logger;

        public TokenService(
            DND5EContext context,
            UserManager<IdentityUser> userManager,
            IConfiguration configuration,
            ILogger<TokenService> logger)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<TokenResponse> GenerateTokensAsync(IdentityUser user)
        {
            if (user == null)
            {
                _logger.LogError("Attempt to generate tokens for null user");
                throw new ArgumentNullException(nameof(user));
            }

            // Generate access token
            var jwtToken = await GenerateJwtToken(user);

            // Generate refresh token
            var refreshToken = GenerateRefreshToken();

            var salt = generateSalt();
            var argon2Config = new Argon2Config
            {
                Password = Encoding.UTF8.GetBytes(refreshToken),
                Salt = salt,
                TimeCost = 3,
                MemoryCost = 65536,
                Threads = 4,
                Type = Argon2Type.HybridAddressing
            };
            var refreshTokenHash = Argon2.Hash(argon2Config);

            // Save refresh token
            await _context.RefreshTokens.AddAsync(new RefreshTokenModel
            {
                UserId = user.Id,
                TokenHash = refreshTokenHash,
                Salt = Convert.ToBase64String(salt),
                AddedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["Jwt:RefreshTokenExpiryInDays"])),
                IsRevoked = false,
                RevokedAt = null
            });
            await _context.SaveChangesAsync();

            _logger.LogInformation("Tokens generated for user: {UserId}", user.Id);

            return new TokenResponse
            {
                AccessToken = jwtToken,
                RefreshToken = refreshToken,
                Success = true
            };
        }

        public async Task<TokenResponse> RefreshTokensAsync(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogWarning("Refresh token is null or empty");
                throw new ArgumentException("Refresh token is required");
            }

            var storedRefreshTokens = await _context.RefreshTokens
                .Where(rt => !rt.IsRevoked && rt.ExpiryDate >= DateTime.UtcNow)
                .ToListAsync();

            RefreshTokenModel storedRefreshToken = null;
            foreach (var token in storedRefreshTokens)
            {
                var salt = Convert.FromBase64String(token.Salt);
                var argon2Config = new Argon2Config
                {
                    Password = Encoding.UTF8.GetBytes(refreshToken),
                    Salt = salt,
                    TimeCost = 3,
                    MemoryCost = 65536,
                    Threads = 4,
                    Type = Argon2Type.HybridAddressing
                };
                if (Argon2.Verify(token.TokenHash, argon2Config))
                {
                    storedRefreshToken = token;
                    break;
                }
            }

            if (storedRefreshToken == null)
            {
                _logger.LogWarning("Refresh token not found in database");
                throw new SecurityTokenException("Refresh token does not exist");
            }

            if (storedRefreshToken.IsRevoked)
            {
                _logger.LogCritical("Attempt to use revoked refresh token for user: {UserId}. Revoking all tokens.", storedRefreshToken.UserId);
                await RevokeAllTokensAsync(storedRefreshToken.UserId);
                throw new SecurityTokenException("Refresh token has been revoked. All sessions terminated for security reasons.");
            }

            if (storedRefreshToken.ExpiryDate < DateTime.UtcNow)
            {
                _logger.LogWarning("Refresh token expired for user: {UserId}", storedRefreshToken.UserId);
                throw new SecurityTokenException("Refresh token has expired");
            }

            var user = await _userManager.FindByIdAsync(storedRefreshToken.UserId);
            if (user == null)
            {
                _logger.LogWarning("User not found for refresh token: {UserId}", storedRefreshToken.UserId);
                throw new SecurityTokenException("User not found");
            }

            storedRefreshToken.IsRevoked = true;
            storedRefreshToken.RevokedAt = DateTime.UtcNow;
            _context.RefreshTokens.Update(storedRefreshToken);
            await _context.SaveChangesAsync();

            var newTokens = await GenerateTokensAsync(user);

            _logger.LogInformation("Refresh token rotated for user: {UserId}", user.Id);
            return newTokens;
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken, string userId)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                _logger.LogWarning("Attempt to revoke null or empty refresh token");
                return false;
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("User ID is null or empty");
                return false;
            }

            var storedRefreshTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiryDate >= DateTime.UtcNow)
                .ToListAsync();

            RefreshTokenModel storedRefreshToken = null;
            foreach (var token in storedRefreshTokens)
            {
                var salt = Convert.FromBase64String(token.Salt);
                var argon2Config = new Argon2Config
                {
                    Password = Encoding.UTF8.GetBytes(refreshToken),
                    Salt = salt,
                    TimeCost = 3,
                    MemoryCost = 65536,
                    Threads = 4,
                    Type = Argon2Type.HybridAddressing
                };
                if (Argon2.Verify(token.TokenHash, argon2Config))
                {
                    storedRefreshToken = token;
                    break;
                }
            }

            if (storedRefreshToken == null)
            {
                _logger.LogWarning("Refresh token not found for user: {UserId}", userId);
                return false;
            }

            if (storedRefreshToken.IsRevoked)
            {
                _logger.LogWarning("Refresh token already revoked for user: {UserId}", storedRefreshToken.UserId);
                return false;
            }

            storedRefreshToken.IsRevoked = true;
            storedRefreshToken.RevokedAt = DateTime.UtcNow;
            _context.RefreshTokens.Update(storedRefreshToken);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Refresh token revoked for user: {UserId}", storedRefreshToken.UserId);
            return true;
        }

        public async Task<bool> RevokeAllTokensAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("Attempt to revoke tokens for null or empty userId");
                return false;
            }

            var tokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync();

            if (!tokens.Any())
            {
                _logger.LogWarning("No active refresh tokens found for user: {UserId}", userId);
                return false;
            }

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
                _context.RefreshTokens.Update(token);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("All refresh tokens revoked for user: {UserId}", userId);
            return true;
        }

        private async Task<string> GenerateJwtToken(IdentityUser user)
        {
            var configKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(configKey))
            {
                _logger.LogError("JWT Key configuration is missing");
                throw new InvalidOperationException("JWT Key configuration is required");
            }

            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(configKey);

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // Prepare claims
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("id", user.Id)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var now = DateTime.UtcNow;
            var expires = now.AddMinutes(Convert.ToDouble(_configuration["Jwt:AccesTokenExpiryTimeInMinutes"]));

            if (expires <= now)
            {
                _logger.LogWarning("Invalid token expiry configuration. Using default 60 minutes.");
                expires = now.AddMinutes(60);
            }

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                notBefore: now,
                expires: expires,
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            );

            _logger.LogDebug("JWT token generated with NotBefore: {NotBefore}, Expires: {Expires}", now, expires);
            return jwtTokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes);
        }

        private byte[] generateSalt()
        {
            var bytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return bytes;
        }
    }
}