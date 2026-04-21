using FinanceTrack.Finance.UseCases.FinancialTransactions.List;
using FinanceTrack.Finance.Web.Extensions;
using FinanceTrack.Finance.Web.Transactions;
using FluentValidation;

namespace FinanceTrack.Finance.Web.FinancialTransactions;

public class ListWalletTransactionsRequest
{
    public const string Route = "/Wallets/{WalletId:guid}/Transactions";
    public Guid WalletId { get; set; }

    [QueryParam]
    public DateOnly From { get; set; }

    [QueryParam]
    public DateOnly To { get; set; }

    [QueryParam]
    public string? AfterCursor { get; set; }

    [QueryParam]
    public int PageSize { get; set; }
}

public class ListWalletTransactionsValidator : Validator<ListWalletTransactionsRequest>
{
    public ListWalletTransactionsValidator()
    {
        RuleFor(r => r.WalletId).NotEmpty();
        RuleFor(r => r.From).LessThanOrEqualTo(r => r.To);
        RuleFor(r => r.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
    }
}

public class ListWalletTransactionsResponse
{
    public List<FinancialTransactionRecord> Transactions { get; set; } = [];
    public string? NextCursor { get; set; }
    public bool HasMore { get; set; }
}

public class ListWalletTransactions(IMediator mediator)
    : Endpoint<ListWalletTransactionsRequest, ListWalletTransactionsResponse>
{
    public override void Configure()
    {
        Get(ListWalletTransactionsRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(
        ListWalletTransactionsRequest req,
        CancellationToken cancel
    )
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(cancel);
            return;
        }

        var query = new ListWalletTransactionsQuery(
            userId,
            req.WalletId,
            req.From,
            req.To,
            req.AfterCursor,
            req.PageSize
        );
        var result = await mediator.Send(query, cancel);

        if (await this.SendResultIfNotOk(result, cancel))
            return;

        await SendOkAsync(
            new ListWalletTransactionsResponse
            {
                Transactions = result
                    .Value.Transactions.Select(t => new FinancialTransactionRecord(
                        t.Id,
                        t.WalletId,
                        t.Name,
                        t.Description,
                        t.Amount,
                        t.OperationDate,
                        t.Type,
                        t.CategoryId,
                        t.RelatedTransactionId,
                        t.RecurringTransactionId,
                        t.RelatedWalletId,
                        t.RelatedWalletName
                    ))
                    .ToList(),
                NextCursor = result.Value.NextCursor,
                HasMore = result.Value.HasMore,
            },
            cancel
        );
    }
}
