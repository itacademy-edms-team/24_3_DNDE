using FinanceTrack.Finance.UseCases.Analytics;
using FinanceTrack.Finance.UseCases.Analytics.Dto;
using FinanceTrack.Finance.Web.Extensions;
using FluentValidation;

namespace FinanceTrack.Finance.Web.FinancialTransactions.Meta;

public sealed class GetWalletTransactionsDateRangeRequest
{
    public const string Route = "/Wallets/{WalletId:guid}/Transactions/Meta/DateRange";
    public Guid WalletId { get; set; }
}

public class GetWalletTransactionsDateRangeValidator
    : Validator<GetWalletTransactionsDateRangeRequest>
{
    public GetWalletTransactionsDateRangeValidator()
    {
        RuleFor(r => r.WalletId).NotEmpty();
    }
}

public class GetWalletTransactionsDateRange(IMediator mediator)
    : Endpoint<GetWalletTransactionsDateRangeRequest, DateMinMaxDto>
{
    public override void Configure()
    {
        Get(GetWalletTransactionsDateRangeRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(
        GetWalletTransactionsDateRangeRequest req,
        CancellationToken cancel
    )
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(cancel);
            return;
        }

        var result = await mediator.Send(
            new GetWalletDateMinMaxQuery(userId, req.WalletId),
            cancel
        );

        if (await this.SendResultIfNotOk(result, cancel))
            return;

        await SendOkAsync(result.Value, cancel);
    }
}
