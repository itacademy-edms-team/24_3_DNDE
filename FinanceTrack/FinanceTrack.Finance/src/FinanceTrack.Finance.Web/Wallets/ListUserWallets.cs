using FinanceTrack.Finance.UseCases.Wallets;
using FinanceTrack.Finance.UseCases.Wallets.List;
using FinanceTrack.Finance.Web.Extensions;
using FluentValidation;

namespace FinanceTrack.Finance.Web.Wallets;

public class ListUserWalletsRequest
{
    [QueryParam]
    public string? AfterCursor { get; set; }

    [QueryParam]
    public int PageSize { get; set; }
}

public class ListUserWalletsValidator : Validator<ListUserWalletsRequest>
{
    public ListUserWalletsValidator()
    {
        RuleFor(r => r.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
    }
}

public class ListUserWalletsResponse
{
    public List<WalletRecord> Wallets { get; set; } = [];
    public string? NextCursor { get; set; }
    public bool HasMore { get; set; }
}

public class ListUserWallets(IMediator mediator)
    : Endpoint<ListUserWalletsRequest, ListUserWalletsResponse>
{
    public override void Configure()
    {
        Get("/Wallets");
        Roles("user");
    }

    public override async Task HandleAsync(ListUserWalletsRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var result = await mediator.Send(
            new ListUserWalletsQuery(userId, req.AfterCursor, req.PageSize),
            ct
        );

        if (await this.SendResultIfNotOk(result, ct))
            return;

        await SendOkAsync(
            new ListUserWalletsResponse
            {
                Wallets = result
                    .Value.Wallets.Select(w => new WalletRecord(
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
                NextCursor = result.Value.NextCursor,
                HasMore = result.Value.HasMore,
            },
            ct
        );
    }
}
