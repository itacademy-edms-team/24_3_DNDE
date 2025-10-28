using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Idenitity.Infrastructure.Services.Jwt;
using Identity.Application.Exceptions;
using Identity.Application.Ports.Repositories;
using Identity.Application.Ports.Services;
using Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Idenitity.Infrastructure.Services.Jwt
{
    public class JwtService(
        IOptions<JwtOptions> jwtOptions,
        IRefreshTokenRepository refreshTokenRepository,
        UserManager<User> userManager
    ) : IAuthTokenService
    {
        public async Task<string> GenerateAccessTokenAsync(User user)
        {
            var roles = await userManager.GetRolesAsync(user);
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            } // Add roles to claims
                .Concat(roles.Select(role => new Claim(ClaimTypes.Role, role)))
                .ToArray();

            // Token generation
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.Value.AccessTokenOptions.SigningKey)
            );
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtOptions.Value.AccessTokenOptions.Issuer,
                audience: jwtOptions.Value.AccessTokenOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    jwtOptions.Value.AccessTokenOptions.LifeTimeInMinutes
                ),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> GenerateRefreshTokenAsync(User user)
        {
            var token = Guid.NewGuid().ToString();

            var dtUtcNow = DateTime.UtcNow;
            var refreshTokenModel = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = token,
                Created = dtUtcNow,
                Expires = dtUtcNow.AddMinutes(
                    jwtOptions.Value.RefreshTokenOptions.LifeTimeInMinutes
                ),
                IsRevoked = false,
                User = user,
            };
            await refreshTokenRepository.CreateRefreshTokenAsync(refreshTokenModel);

            return token;
        }
    }
}
