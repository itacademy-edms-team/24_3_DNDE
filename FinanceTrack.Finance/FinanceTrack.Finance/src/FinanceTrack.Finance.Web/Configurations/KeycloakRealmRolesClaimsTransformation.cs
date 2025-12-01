using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace FinanceTrack.Finance.Web.Configurations;

/// <summary>
/// Take roles from realm_access.roles and add it to ClaimsIdentity as Role-claims
/// </summary>
public class KeycloakRealmRolesClaimsTransformation(
  ILogger<KeycloakRealmRolesClaimsTransformation> logger
) : IClaimsTransformation
{
  public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
  {
    if (principal.Identity is not ClaimsIdentity identity)
      return Task.FromResult(principal);

    var realmAccess = identity.FindFirst("realm_access");
    if (realmAccess is null)
      return Task.FromResult(principal);

    try
    {
      using var doc = JsonDocument.Parse(realmAccess.Value);
      if (
        doc.RootElement.TryGetProperty("roles", out var rolesElement)
        && rolesElement.ValueKind == JsonValueKind.Array
      )
      {
        foreach (var roleJson in rolesElement.EnumerateArray())
        {
          var roleName = roleJson.GetString();
          if (
            !string.IsNullOrWhiteSpace(roleName)
            && !identity.HasClaim(identity.RoleClaimType, roleName)
          )
          {
            identity.AddClaim(new Claim(identity.RoleClaimType, roleName));
          }
        }
      }
    }
    catch (Exception ex)
    {
      logger.LogWarning("Unexpected exception: {Exception}", ex);
    }

    return Task.FromResult(principal);
  }
}
