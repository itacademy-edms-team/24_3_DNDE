using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Idenitity.Infrastructure.Services.Jwt;
using Identity.Application.Exceptions;
using Identity.Application.Ports.Services;
using Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Idenitity.Infrastructure.Services.Jwt
{
    public class JwtService : IAuthTokenService
    {
        private readonly JwtSettings _settings;
        private readonly RsaSecurityKey _rsaSecurityKey;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public JwtService(
            IOptions<JwtSettings> settings,
            RsaSecurityKey rsaSecurityKey,
            UserManager<User> userManager,
            SignInManager<User> signInManager
        )
        {
            _settings = settings.Value;
            _rsaSecurityKey = rsaSecurityKey;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public Task<string> GenerateAccessToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var key = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(_settings.AccessTokenSettings.SigningKey)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _settings.AccessTokenSettings.Issuer,
                audience: _settings.AccessTokenSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddSeconds(
                    _settings.AccessTokenSettings.LifeTimeInSeconds
                ),
                signingCredentials: creds
            );

            return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
        }

        public Task<string> GenerateRefreshToken()
        {
            var size = _settings.RefreshTokenSettings.Length;
            var buffer = new byte[size];
            using var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(buffer);
            return Task.FromResult(Convert.ToBase64String(buffer));
        }

        public Task RevokeAccessToken(User user) { }
    }
}
