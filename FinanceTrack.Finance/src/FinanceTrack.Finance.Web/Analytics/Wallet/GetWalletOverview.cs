using FinanceTrack.Finance.UseCases.Analytics.Dto;
using FinanceTrack.Finance.UseCases.Analytics.Wallet;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.Analytics.Wallet;

public class GetWalletOverviewRequest
{
    public const string Route = "/Analytics/Wallets/{WalletId:guid}/Overview";
    public Guid WalletId { get; set; }

    [QueryParam]
    public DateOnly From { get; set; }

    [QueryParam]
    public DateOnly To { get; set; }
}

public class GetWalletOverview(IMediator mediator)
    : Endpoint<GetWalletOverviewRequest, WalletOverviewDto>
{
    public override void Configure()
    {
        Get(GetWalletOverviewRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(GetWalletOverviewRequest req, CancellationToken cancel)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(cancel);
            return;
        }

        var result = await mediator.Send(
            new GetWalletOverviewQuery(userId, req.WalletId, req.From, req.To),
            cancel
        );

        if (await this.SendResultIfNotOk(result, cancel))
            return;

        await SendOkAsync(result.Value, cancel);
    }
}
