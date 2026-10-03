using FinanceTrack.Finance.UseCases.Wallets.Get;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.Wallets;

public class GetWalletRequest
{
    public const string Route = "/Wallets/{WalletId:guid}";
    public Guid WalletId { get; set; }
}

public class GetWallet(IMediator mediator) : Endpoint<GetWalletRequest, WalletRecord>
{
    public override void Configure()
    {
        Get(GetWalletRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(GetWalletRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var result = await mediator.Send(new GetWalletQuery(req.WalletId, userId), ct);

        if (await this.SendResultIfNotOk(result, ct))
            return;

        var w = result.Value;
        Response = new WalletRecord(
            w.Id, w.Name, w.WalletType, w.Balance,
            w.AllowNegativeBalance, w.TargetAmount, w.TargetDate, w.IsArchived
        );
    }
}
