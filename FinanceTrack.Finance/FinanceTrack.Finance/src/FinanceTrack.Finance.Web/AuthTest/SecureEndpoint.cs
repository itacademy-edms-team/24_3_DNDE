namespace FinanceTrack.Finance.Web.AuthTest;

public class SecureEndpoint : EndpointWithoutRequest<string>
{
  public override void Configure()
  {
    Get("/auth-test/secure");
    // Require 'user' role
    Roles("user");
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var name = User?.Identity?.Name ?? "unknown";
    await SendAsync(
      $"Привет, {name}! У тебя есть роль 'user' в FinanceTrack.Finance.",
      cancellation: ct
    );
  }
}
