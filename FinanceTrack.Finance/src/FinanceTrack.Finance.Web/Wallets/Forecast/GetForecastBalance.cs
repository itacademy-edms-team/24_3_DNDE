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

    public override async Task HandleAsync(GetForecastBalanceRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var result = await mediator.Send(new GetForecastBalanceQuery(userId, req.WalletId), ct);

        if (await this.SendResultIfNotOk(result, ct))
            return;

        await SendOkAsync(result.Value, ct);
    }
}
