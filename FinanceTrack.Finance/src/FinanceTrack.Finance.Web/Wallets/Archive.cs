using FinanceTrack.Finance.UseCases.Wallets.Archive;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.Wallets;

public class ArchiveWalletRequest
{
    public const string Route = "/Wallets/{WalletId:guid}/archive";
    public Guid WalletId { get; set; }
}

public class ArchiveWallet(IMediator mediator) : Endpoint<ArchiveWalletRequest>
{
    public override void Configure()
    {
        Post(ArchiveWalletRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(ArchiveWalletRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var result = await mediator.Send(new ArchiveWalletCommand(req.WalletId, userId), ct);

        if (await this.SendResultIfNotOk(result, ct))
            return;

        await SendNoContentAsync(ct);
    }
}
