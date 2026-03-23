using FinanceTrack.Finance.UseCases.Analytics.Dto;
using FinanceTrack.Finance.UseCases.Analytics.Wallet;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.Analytics.Wallet;

public class GetWalletCategoriesAnalyticsRequest
{
    public const string Route = "/Analytics/Wallets/{WalletId:guid}/Categories";
    public Guid WalletId { get; set; }

    [QueryParam]
    public DateOnly From { get; set; }

    [QueryParam]
    public DateOnly To { get; set; }
}

public class GetWalletCategoriesAnalytics(IMediator mediator)
    : Endpoint<GetWalletCategoriesAnalyticsRequest, CategoriesAnalyticsDto>
{
    public override void Configure()
    {
        Get(GetWalletCategoriesAnalyticsRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(
        GetWalletCategoriesAnalyticsRequest req,
        CancellationToken ct
    )
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var result = await mediator.Send(
            new GetWalletCategoriesAnalyticsQuery(userId, req.WalletId, req.From, req.To),
            ct
        );

        if (await this.SendResultIfNotOk(result, ct))
            return;

        await SendOkAsync(result.Value, ct);
    }
}
