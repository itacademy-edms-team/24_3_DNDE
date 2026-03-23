using FinanceTrack.Finance.UseCases.Analytics;
using FinanceTrack.Finance.UseCases.Analytics.Dto;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.Analytics.Wallet;

public sealed class GetWalletYearMinMaxRequest
{
    public const string Route = "/Analytics/Wallets/{WalletId:guid}/Meta/YearMinMax";
    public Guid WalletId { get; set; }
}

public class GetWalletYearMinMax(IMediator mediator)
    : Endpoint<GetWalletYearMinMaxRequest, YearMinMaxDto>
{
    public override void Configure()
    {
        Get(GetWalletYearMinMaxRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(GetWalletYearMinMaxRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var result = await mediator.Send(new GetWalletYearMinMaxQuery(userId, req.WalletId), ct);

        if (await this.SendResultIfNotOk(result, ct))
            return;

        await SendOkAsync(result.Value, ct);
    }
}
