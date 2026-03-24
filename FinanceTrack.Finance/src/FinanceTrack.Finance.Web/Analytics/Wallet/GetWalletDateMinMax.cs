using FinanceTrack.Finance.UseCases.Analytics;
using FinanceTrack.Finance.UseCases.Analytics.Dto;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.Analytics.Wallet;

public sealed class GetWalletDateMinMaxRequest
{
    public const string Route = "/Analytics/Wallets/{WalletId:guid}/Meta/DateMinMax";
    public Guid WalletId { get; set; }
}

public class GetWalletDateMinMax(IMediator mediator)
    : Endpoint<GetWalletDateMinMaxRequest, DateMinMaxDto>
{
    public override void Configure()
    {
        Get(GetWalletDateMinMaxRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(GetWalletDateMinMaxRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var result = await mediator.Send(new GetWalletDateMinMaxQuery(userId, req.WalletId), ct);

        if (await this.SendResultIfNotOk(result, ct))
            return;

        await SendOkAsync(result.Value, ct);
    }
}
