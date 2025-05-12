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
        Task<AuthResponse> GenerateTokensAsync(IdentityUser user);
        Task<AuthResponse> VerifyAndGenerateTokenAsync(TokenRequest tokenRequest);
        Task<bool> RevokeTokenAsync(string token);
    }

    public class TokenService: ITokenService
    {
        private readonly DND5EContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly TokenValidationParameters _tokenValidationParameters;

        public TokenService(
            DND5EContext context,
            UserManager<IdentityUser> userManager,
            IConfiguration configuration,
            TokenValidationParameters tokenValidationParameters)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            _tokenValidationParameters = tokenValidationParameters;
        }

        public async Task<AuthResponse> GenerateTokensAsync(IdentityUser user)
        {
            // Generate acces token
            var jwtToken = await GenerateJwtToken(user);
            // Using access token generate refresh token
            var refreshToken = GenerateRefreshToken(jwtToken);
            refreshToken.UserId = user.Id;

            // Save changes
            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            // Return response
            return new AuthResponse()
            {
                AccessToken = jwtToken,
                RefreshToken = refreshToken.Token,
                Success = true
            };
        }

        public async Task<AuthResponse> VerifyAndGenerateTokenAsync(TokenRequest tokenRequest)
        {
            // Checking JWT validity
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            try
            {
                // Check token format
                var tokenInVerification = jwtTokenHandler.ValidateToken(
                    tokenRequest.AccessToken,
                    _tokenValidationParameters,
                    out var validatedToken);

                // Check signin credentials
                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(
                        SecurityAlgorithms.HmacSha256,
                        StringComparison.InvariantCultureIgnoreCase);

                    if (!result)
                        return new AuthResponse { Success = false, Errors = new[] { "Неверный алгоритм подписи токена" } };
                }

                // Check expiration date
                var utcExpiryDate = long.Parse(
                    tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

                var expiryDate = UnixTimeStampToDateTime(utcExpiryDate);

                // Check if token in DB
                var storedRefreshToken = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == tokenRequest.RefreshToken);

                if (storedRefreshToken == null)
                    return new AuthResponse { Success = false, Errors = new[] { "Refresh токен не существует" } };

                // Check if token is already used
                if (storedRefreshToken.IsUsed)
                    return new AuthResponse { Success = false, Errors = new[] { "Refresh токен уже использован" } };

                // Check if token is revoked
                if (storedRefreshToken.IsRevoked)
                    return new AuthResponse { Success = false, Errors = new[] { "Refresh токен был отозван" } };

                // Get JWT ID from token
                var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

                // Compare JWT ID with stored refresh token
                if (storedRefreshToken.JwtId != jti)
                    return new AuthResponse { Success = false, Errors = new[] { "Refresh токен не соответствует JWT токену" } };

                // Check refresh token expiry date
                if (storedRefreshToken.ExpiryDate < DateTime.UtcNow)
                    return new AuthResponse { Success = false, Errors = new[] { "Срок действия refresh токена истек" } };

                // Update status of valid refresh token
                storedRefreshToken.IsUsed = true;
                _context.RefreshTokens.Update(storedRefreshToken);
                await _context.SaveChangesAsync();

                // Get user by ID
                var userId = tokenInVerification.Claims.SingleOrDefault(x => x.Type == "id").Value;
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                    return new AuthResponse { Success = false, Errors = new[] { "Пользователь не найден" } };

                // Generate new token pair
                return await GenerateTokensAsync(user);
            }
            catch (Exception ex)
            {
                return new AuthResponse { Success = false, Errors = new[] { "Произошла ошибка при проверке токена: " + ex.Message } };
            }
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {
            // Check if token exists
            var token = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (token == null)
                return false;

            // If token already used or revoked, return false
            if (token.IsUsed || token.IsRevoked)
                return false;

            // else, revoke token
            token.IsRevoked = true;
            _context.RefreshTokens.Update(token);
            await _context.SaveChangesAsync();

            return true;
        }
        
        private async Task<string> GenerateJwtToken(IdentityUser user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

            // Get user role
            var roles = await _userManager.GetRolesAsync(user);

            // Prepare claims for token
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("id", user.Id)
            };

            // Add roles to claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Configuring token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:AccesTokenExpiryTimeInMinutes"])),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            return jwtToken;
        }

        private RefreshTokenModel GenerateRefreshToken(string jwtToken)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var parsedToken = jwtTokenHandler.ReadJwtToken(jwtToken);
            var jti = parsedToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

            return new RefreshTokenModel
            {
                Token = RandomStringGenerator(35) + Guid.NewGuid().ToString(),
                JwtId = jti,
                IsUsed = false,
                IsRevoked = false,
                AddedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["Jwt:RefreshTokenExpiryInDays"])),
            };
        }

        private DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            var dateTimeVal = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTimeVal = dateTimeVal.AddSeconds(unixTimeStamp);
            return dateTimeVal;
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
