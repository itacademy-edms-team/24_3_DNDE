using FinanceTrack.Finance.UseCases.Analytics;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.Analytics;

public class GetAccountAnalyticsRequest
{
    public const string Route = "/Analytics/Wallets/{WalletId:guid}";
    public Guid WalletId { get; set; }
    [QueryParam] public DateOnly From { get; set; }
    [QueryParam] public DateOnly To { get; set; }
}

public class GetAccountAnalytics(IMediator mediator)
    : Endpoint<GetAccountAnalyticsRequest, AccountAnalyticsDto>
{
    public override void Configure()
    {
        Get(GetAccountAnalyticsRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(GetAccountAnalyticsRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var result = await mediator.Send(
            new GetAccountAnalyticsQuery(userId, req.WalletId, req.From, req.To),
            ct
        );

        if (await this.SendResultIfNotOk(result, ct))
            return;

        await SendOkAsync(result.Value, ct);
    }
}
