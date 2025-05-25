using DND5E_CE.Server.Data;
using DND5E_CE.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DND5E_CE.Server.Services
{
    public interface ITokenService
    {
        Task<TokenResponse> GenerateTokensAsync(IdentityUser user);
        Task<TokenResponse> VerifyAndGenerateTokenAsync(TokenRequest tokenRequest);
        Task<bool> RevokeTokenAsync(string token);
    }

    public class TokenService : ITokenService
    {
        private readonly DND5EContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly ILogger<TokenService> _logger;

        public TokenService(
            DND5EContext context,
            UserManager<IdentityUser> userManager,
            IConfiguration configuration,
            TokenValidationParameters tokenValidationParameters,
            ILogger<TokenService> logger)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            _tokenValidationParameters = tokenValidationParameters;
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
            var refreshToken = GenerateRefreshToken(jwtToken);
            refreshToken.UserId = user.Id;

            // Save refresh token
            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Tokens generated for user: {UserId}", user.Id);

            return new TokenResponse
            {
                AccessToken = jwtToken,
                RefreshToken = refreshToken.Token,
                Success = true
            };
        }

        public async Task<TokenResponse> VerifyAndGenerateTokenAsync(TokenRequest tokenRequest)
        {
            if (tokenRequest == null || string.IsNullOrEmpty(tokenRequest.AccessToken) || string.IsNullOrEmpty(tokenRequest.RefreshToken))
            {
                _logger.LogWarning("Invalid token request: access or refresh token is null or empty");
                throw new ArgumentException("Access token and refresh token are required");
            }

            var jwtTokenHandler = new JwtSecurityTokenHandler();

            try
            {
                // Validate JWT
                var tokenInVerification = jwtTokenHandler.ValidateToken(
                    tokenRequest.AccessToken,
                    _tokenValidationParameters,
                    out var validatedToken);

                // Check signing algorithm
                if (!(validatedToken is JwtSecurityToken jwtSecurityToken &&
                      jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase)))
                {
                    _logger.LogWarning("Invalid JWT signing algorithm for token: {Token}", tokenRequest.AccessToken);
                    throw new SecurityTokenException("Invalid token signing algorithm");
                }

                // Check token in database
                var storedRefreshToken = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == tokenRequest.RefreshToken)
                    ?? throw new SecurityTokenException("Refresh token does not exist");

                // Check if token is used
                if (storedRefreshToken.IsUsed)
                {
                    _logger.LogWarning("Refresh token already used: {Token}", storedRefreshToken.Token);
                    throw new SecurityTokenException("Refresh token already used");
                }

                // Check if token is revoked
                if (storedRefreshToken.IsRevoked)
                {
                    _logger.LogWarning("Refresh token revoked: {Token}", storedRefreshToken.Token);
                    throw new SecurityTokenException("Refresh token has been revoked");
                }

                // Check JWT ID
                var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value
                    ?? throw new SecurityTokenException("JWT ID claim missing");
                if (storedRefreshToken.JwtId != jti)
                {
                    _logger.LogWarning("Refresh token does not match JWT ID for token: {Token}", storedRefreshToken.Token);
                    throw new SecurityTokenException("Refresh token does not match JWT token");
                }

                // Check refresh token expiry
                if (storedRefreshToken.ExpiryDate < DateTime.UtcNow)
                {
                    _logger.LogWarning("Refresh token expired: {Token}", storedRefreshToken.Token);
                    throw new SecurityTokenException("Refresh token has expired");
                }

                // Mark token as used
                storedRefreshToken.IsUsed = true;
                _context.RefreshTokens.Update(storedRefreshToken);
                await _context.SaveChangesAsync();

                // Get user
                var userId = tokenInVerification.Claims.FirstOrDefault(x => x.Type == "id")?.Value
                    ?? throw new SecurityTokenException("User ID claim missing");
                var user = await _userManager.FindByIdAsync(userId)
                    ?? throw new SecurityTokenException("User not found");

                _logger.LogInformation("Refresh token verified, generating new tokens for user: {UserId}", userId);

                // Generate new tokens
                return await GenerateTokensAsync(user);
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning("Token verification failed: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during token verification");
                throw new InvalidOperationException("Error verifying token", ex);
            }
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogWarning("Attempt to revoke null or empty refresh token");
                throw new ArgumentException("Refresh token is required");
            }

            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (token == null)
            {
                _logger.LogWarning("Refresh token not found: {Token}", refreshToken);
                return false;
            }

            // If token already used or revoked, return false
            if (token.IsUsed || token.IsRevoked)
            {
                _logger.LogWarning("Refresh token already used or revoked: {Token}", refreshToken);
                return false;
            }

            // else, revoke token
            token.IsRevoked = true;
            _context.RefreshTokens.Update(token);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Refresh token revoked: {Token}", refreshToken);
            return true;
        }
        
        private async Task<string> GenerateJwtToken(IdentityUser user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // Prepare claims
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("id", user.Id)
            };

            // Add roles to claims
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

            _logger.LogDebug("JWT token generated with NotBefore: {NotBefore}, Expires: {Expires}",
                now, expires);

            return jwtTokenHandler.WriteToken(token);
        }

        private RefreshTokenModel GenerateRefreshToken(string jwtToken)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var parsedToken = jwtTokenHandler.ReadJwtToken(jwtToken);
            var jti = parsedToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value
                ?? throw new InvalidOperationException("JWT ID claim missing in token");

            return new RefreshTokenModel
            {
                Token = RandomStringGenerator(35) + Guid.NewGuid().ToString(),
                JwtId = jti,
                IsUsed = false,
                IsRevoked = false,
                AddedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["Jwt:RefreshTokenExpiryInDays"]))
            };
        }

        private DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            return DateTime.UnixEpoch.AddSeconds(unixTimeStamp);
        }

        private string RandomStringGenerator(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return string.Create(length, Random.Shared, (span, random) =>
            {
                for (int i = 0; i < span.Length; i++)
                    span[i] = chars[random.Next(chars.Length)];
            });
        }
    }
}
