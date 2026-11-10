using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Tests.Integration;

/// <summary>
/// Типы claim'ов, по которым монолит ищет имя и роли, и их фактическая работа.
/// Защита от регрессии: при неверном типе проверки молча отказывают - User.Identity.Name вернёт null,
/// IsInRole - false, [Authorize(Roles)] - 403 залогиненному пользователю, без ошибок и записей в лог.
/// </summary>
public sealed class JwtBearerOptionsTests : IDisposable
{
    private readonly AppFactory _app = new();

    private JwtBearerOptions Options =>
        _app.Services.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

    public void Dispose() => _app.Dispose();

    [Fact]
    public void Options_DefaultConfiguration_KeepsRawClaimNames()
    {
        Assert.False(Options.MapInboundClaims);
    }

    [Fact]
    public void Options_DefaultConfiguration_UsesFullNameClaim()
    {
        Assert.Equal("name", Options.TokenValidationParameters.NameClaimType);
    }

    [Fact]
    public void Options_DefaultConfiguration_UsesFlatRolesClaim()
    {
        Assert.Equal("roles", Options.TokenValidationParameters.RoleClaimType);
    }

    [Fact]
    public void Options_DefaultConfiguration_ValidatesApiAudience()
    {
        Assert.True(Options.TokenValidationParameters.ValidateAudience);
        Assert.Equal("RecallApp", Options.TokenValidationParameters.ValidAudience);
    }

    [Fact]
    public async Task ValidateToken_TokenWithPrefixedRole_PopulatesNameAndRole()
    {
        using var rsa = RSA.Create(2048);
        var token = CreateToken(
            rsa,
            "RecallApp",
            new Claim("name", "test user"),
            new Claim("roles", "RecallApp:app-user")
        );

        var principal = await ValidateAsync(token, rsa);

        Assert.True(principal.Identity?.IsAuthenticated);
        Assert.Equal("test user", principal.Identity?.Name);
        Assert.True(principal.IsInRole("RecallApp:app-user"));
    }

    [Fact]
    public async Task ValidateToken_UnprefixedRole_DoesNotSatisfyPrefixedCheck()
    {
        using var rsa = RSA.Create(2048);
        var token = CreateToken(rsa, "RecallApp", new Claim("roles", "RecallApp:app-user"));

        var principal = await ValidateAsync(token, rsa);

        // Контракт ролей включает префикс клиента: [Authorize(Roles = "app-user")] не должен срабатывать.
        Assert.False(principal.IsInRole("app-user"));
    }

    [Fact]
    public async Task ValidateToken_TokenWithAdditionalAudience_Accepted()
    {
        using var rsa = RSA.Create(2048);
        var token = CreateToken(rsa, new[] { "RecallApp", "account" }, new Claim("sub", "user-1"));
        
        var result = await new JsonWebTokenHandler().ValidateTokenAsync(token, ValidationParameters(rsa));

        // aud - множество: достаточно присутствия нашего идентификатора.
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateToken_TokenWithoutApiAudience_Rejected()
    {
        using var rsa = RSA.Create(2048);
        var token = CreateToken(rsa, new[] { "account" }, new Claim("sub", "user-1"));
        
        var result = await new JsonWebTokenHandler().ValidateTokenAsync(token, ValidationParameters(rsa));

        Assert.False(result.IsValid);
    }

    /// <summary>
    /// Параметры берём из конфигурации приложения (в первую очередь типы claim'ов),
    /// подменяя только то, что относится к тестовому ключу и сети.
    /// </summary>
    private TokenValidationParameters ValidationParameters(RSA rsa)
    {
        var configured = Options.TokenValidationParameters;
        return new TokenValidationParameters
        {
            NameClaimType = configured.NameClaimType,
            RoleClaimType = configured.RoleClaimType,
            ValidateAudience = configured.ValidateAudience,
            ValidAudience = configured.ValidAudience,
            ValidateIssuer = false,
            ValidateLifetime = false,
            IssuerSigningKey = new RsaSecurityKey(rsa),
        };
    }

    private async Task<ClaimsPrincipal> ValidateAsync(string token, RSA rsa)
    {
        var result = await new JsonWebTokenHandler().ValidateTokenAsync(token, ValidationParameters(rsa));

        Assert.True(result.IsValid);
        return new ClaimsPrincipal(result.ClaimsIdentity);
    }

    private static string CreateToken(RSA rsa, object audience, params Claim[] claims) =>
        new JsonWebTokenHandler().CreateToken(
            new SecurityTokenDescriptor
            {
                Issuer = "https://auth.localhost/realms/recallapp",
                Subject = new ClaimsIdentity(claims, "Test"),
                // aud задаётся явно: в Keycloak это строка, но по RFC 7519 допускается и множество.
                Claims = new Dictionary<string, object> { ["aud"] = audience },
                SigningCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256),
            }
        );
}
