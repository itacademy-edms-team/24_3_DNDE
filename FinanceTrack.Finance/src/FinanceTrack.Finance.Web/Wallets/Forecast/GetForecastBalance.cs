using FinanceTrack.Finance.UseCases.Wallets;
using FinanceTrack.Finance.UseCases.Wallets.GetForecastBalance;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.Wallets.Forecast;

public class GetForecastBalanceRequest
{
    public const string Route = "/Wallets/{WalletId:guid}/Forecast/Balance";
    public Guid WalletId { get; set; }
}

public class GetForecastBalance(IMediator mediator)
    : Endpoint<GetForecastBalanceRequest, WalletForecastBalanceDto>
{
    public override void Configure()
    {
        Get(GetForecastBalanceRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(GetForecastBalanceRequest req, CancellationToken cancel)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(cancel);
            return;
        }

        var result = await mediator.Send(new GetForecastBalanceQuery(userId, req.WalletId), cancel);

        if (await this.SendResultIfNotOk(result, cancel))
            return;

        await SendOkAsync(result.Value, cancel);
    }
}
