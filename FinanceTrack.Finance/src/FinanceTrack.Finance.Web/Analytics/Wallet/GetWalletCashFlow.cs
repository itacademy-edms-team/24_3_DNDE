using FinanceTrack.Finance.UseCases.Analytics.Dto;
using FinanceTrack.Finance.UseCases.Analytics.Wallet;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.Analytics.Wallet;

public class GetWalletCashFlowRequest
{
    public const string Route = "/Analytics/Wallets/{WalletId:guid}/CashFlow";
    public Guid WalletId { get; set; }

    [QueryParam]
    public DateOnly From { get; set; }

    [QueryParam]
    public DateOnly To { get; set; }
}

public class GetWalletCashFlow(IMediator mediator) : Endpoint<GetWalletCashFlowRequest, CashFlowDto>
{
    public override void Configure()
    {
        Get(GetWalletCashFlowRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(GetWalletCashFlowRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var result = await mediator.Send(
            new GetWalletCashFlowQuery(userId, req.WalletId, req.From, req.To),
            ct
        );

        if (await this.SendResultIfNotOk(result, ct))
            return;

        await SendOkAsync(result.Value, ct);
    }
}
