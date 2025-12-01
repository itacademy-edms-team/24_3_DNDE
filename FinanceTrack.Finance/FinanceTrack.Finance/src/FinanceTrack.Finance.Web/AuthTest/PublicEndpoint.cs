namespace FinanceTrack.Finance.Web.AuthTest;

public class PublicEndpoint : EndpointWithoutRequest<string>
{
  public override void Configure()
  {
    Get("/api/finance/auth-test/public");
    AllowAnonymous();
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    await SendAsync("Публичный эндпоинт FinanceTrack.Finance", cancellation: ct);
  }
}
