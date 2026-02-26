using FinanceTrack.Finance.UseCases.Wallets.List;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.Wallets;

public class ListUserArchiveWalletsResponse
{
    public List<WalletRecord> Wallets { get; set; } = [];
}

public class ListUserArchiveWallets(IMediator mediator)
    : EndpointWithoutRequest<ListUserArchiveWalletsResponse>
{
    public override void Configure()
    {
        Get("/Wallets/Archive");
        Roles("user");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var wallets = await mediator.Send(new ListUserArchiveWalletsQuery(userId), ct);

        Response = new ListUserArchiveWalletsResponse
        {
            Wallets = wallets
                .Select(w => new WalletRecord(
                    w.Id,
                    w.Name,
                    w.WalletType,
                    w.Balance,
                    w.AllowNegativeBalance,
                    w.TargetAmount,
                    w.TargetDate,
                    w.IsArchived
                ))
                .ToList(),
        };
    }
}
