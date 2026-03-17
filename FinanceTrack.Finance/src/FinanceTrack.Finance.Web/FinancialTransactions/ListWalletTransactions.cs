using FinanceTrack.Finance.UseCases.FinancialTransactions.List;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.Transactions;

public class ListWalletTransactionsRequest
{
    public const string Route = "/Wallets/{WalletId:guid}/Transactions";
    public Guid WalletId { get; set; }

    [QueryParam]
    public string? Type { get; set; }

    [QueryParam]
    public DateOnly? From { get; set; }

    [QueryParam]
    public DateOnly? To { get; set; }
}

public class ListWalletTransactionsResponse
{
    public List<FinancialTransactionRecord> Transactions { get; set; } = [];
}

public class ListWalletTransactions(IMediator mediator)
    : Endpoint<ListWalletTransactionsRequest, ListWalletTransactionsResponse>
{
    public override void Configure()
    {
        Get(ListWalletTransactionsRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(ListWalletTransactionsRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var query = new ListWalletTransactionsQuery(
            userId,
            req.WalletId,
            req.Type,
            req.From,
            req.To
        );
        var result = await mediator.Send(query, ct);

        if (await this.SendResultIfNotOk(result, ct))
            return;

        Response = new ListWalletTransactionsResponse
        {
            Transactions = result
                .Value.Select(t => new FinancialTransactionRecord(
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
        };
    }
}
