using System.Security.Claims;
using Ardalis.GuardClauses;
using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Infrastructure;
using FinanceTrack.Finance.Infrastructure.Email;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;

namespace FinanceTrack.Finance.Web.Configurations;

public static class ServiceConfigs
{
  public static IServiceCollection AddServiceConfigs(
    this IServiceCollection services,
    Microsoft.Extensions.Logging.ILogger logger,
    WebApplicationBuilder builder
  )
  {
    services.AddInfrastructureServices(builder.Configuration, logger).AddMediatrConfigs();

    if (builder.Environment.IsDevelopment())
    {
      // Use a local test email server
      // See: https://ardalis.com/configuring-a-local-test-email-server/
      services.AddScoped<IEmailSender, MimeKitEmailSender>();

      // Otherwise use this:
      //builder.Services.AddScoped<IEmailSender, FakeEmailSender>();
    }
    else
    {
      services.AddScoped<IEmailSender, MimeKitEmailSender>();
    }

    logger.LogInformation("{Project} services registered", "Mediatr, Email Sender, Auth");

    AddAuth(services, builder.Configuration, logger);

    return services;
  }

  public static void AddAuth(
    IServiceCollection services,
    IConfiguration configuration,
    Microsoft.Extensions.Logging.ILogger logger
  )
  {
    var authSection = configuration.GetSection("Authentication");
    var authority = Guard.Against.Null(authSection.GetValue<string>("Authority"));
    var requireHttps = Guard.Against.Null(authSection.GetValue<bool>("RequireHttps"));
    
    // Audience validation - set to your API client_id in Keycloak
    var validAudience = authSection.GetValue<string>("Audience");
    var validateAudience = !string.IsNullOrEmpty(validAudience);

    // For keycloak JWT
    services
      .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer(options =>
      {
        options.Authority = authority;
        options.RequireHttpsMetadata = requireHttps;

        options.TokenValidationParameters = new TokenValidationParameters
        {
          // Audience validation - enabled when "Audience" is configured
          ValidateAudience = validateAudience,
          ValidAudience = validAudience,
          ValidIssuer = authority,

          NameClaimType = "preferred_username",
          RoleClaimType = ClaimTypes.Role,
        };

        if (validateAudience)
        {
          logger.LogInformation("JWT audience validation enabled for: {Audience}", validAudience);
        }
        else
        {
          logger.LogWarning("JWT audience validation is DISABLED. Set 'Authentication:Audience' to enable.");
        }
      });

    // realm_access.roles -> Role-claims
    services.AddScoped<IClaimsTransformation, KeycloakRealmRolesClaimsTransformation>();

    services.AddAuthorization();
  }
}
