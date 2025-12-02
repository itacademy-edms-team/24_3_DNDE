namespace FinanceTrack.Finance.Web.AuthTest;

public class DebugUserClaims : EndpointWithoutRequest<object>
{
  public override void Configure()
  {
    Get("/auth-test/claims");
    Roles("user");
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var claims = User.Claims.Select(c => new { c.Type, c.Value });
    await SendAsync(claims, cancellation: ct);
  }
}
