using FinanceTrack.Finance.UseCases.Wallets;
using FinanceTrack.Finance.UseCases.Wallets.List;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.Wallets;

public class ListUserWalletsResponse
{
    public List<WalletRecord> Wallets { get; set; } = [];
}

public class ListUserWallets(IMediator mediator)
    : EndpointWithoutRequest<ListUserWalletsResponse>
{
    public override void Configure()
    {
        Get("/Wallets");
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

        var wallets = await mediator.Send(new ListUserWalletsQuery(userId), ct);

        Response = new ListUserWalletsResponse
        {
            Wallets = wallets
                .Select(w => new WalletRecord(
                    w.Id, w.Name, w.WalletType, w.Balance,
                    w.AllowNegativeBalance, w.TargetAmount, w.TargetDate, w.IsArchived
                ))
                .ToList(),
        };
    }
}
