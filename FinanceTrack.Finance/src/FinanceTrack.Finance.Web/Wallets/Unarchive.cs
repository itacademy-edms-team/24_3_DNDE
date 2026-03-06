using FinanceTrack.Finance.UseCases.Wallets.Unarchive;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.Wallets;

public class UnarchiveWalletRequest
{
    public const string Route = "/Wallets/{WalletId:guid}/unarchive";
    public Guid WalletId { get; set; }
}

public class UnarchiveWallet(IMediator mediator) : Endpoint<UnarchiveWalletRequest>
{
    public override void Configure()
    {
        Post(UnarchiveWalletRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(UnarchiveWalletRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var result = await mediator.Send(new UnarchiveWalletCommand(req.WalletId, userId), ct);

        if (await this.SendResultIfNotOk(result, ct))
            return;

        await SendNoContentAsync(ct);
    }
}
