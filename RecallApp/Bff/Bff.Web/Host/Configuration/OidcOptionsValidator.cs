using Microsoft.Extensions.Options;

namespace Host.Configuration;

/// <summary>
/// Валидатор связанных опций OIDC/refresh.
/// </summary>
public sealed class OidcOptionsValidator : IValidateOptions<OidcOptions>
{
    public ValidateOptionsResult Validate(string? name, OidcOptions options)
    {
        if (options.TokenRefresh.KeycloakTimeout >= options.TokenRefresh.RefreshThreshold)
        {
            return ValidateOptionsResult.Fail(
                "Oidc:TokenRefresh:KeycloakTimeout must be less than Oidc:TokenRefresh:RefreshThreshold: "
                    + "otherwise lock wait can outlive the refresh window and stale tokens overwrite fresh ones "
                    + "(scenario 9 regression)."
            );
        }

        return ValidateOptionsResult.Success;
    }
}
