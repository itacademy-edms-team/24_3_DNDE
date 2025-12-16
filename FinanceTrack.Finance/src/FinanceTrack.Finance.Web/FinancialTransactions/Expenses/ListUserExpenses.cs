using FinanceTrack.Finance.UseCases.FinancialTransactions.Expenses.List;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.Transactions.Expenses;

public class ListUserExpenses(IMediator mediator)
    : Endpoint<ListUserExpensesByIncomeIdRequest, ListExpensesByIncomeIdResponse>
{
    public override void Configure()
    {
        Get(ListUserExpensesByIncomeIdRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(
        ListUserExpensesByIncomeIdRequest request,
        CancellationToken ct
    )
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var expenses = await mediator.Send(
            new ListUserExpenseFinancialTransactionsQuery(userId, request.IncomeTransactionId),
            ct
        );

        if (!expenses.IsSuccess)
        {
            switch (expenses.Status)
            {
                case ResultStatus.NotFound:
                    await SendNotFoundAsync(ct);
                    return;
                case ResultStatus.Forbidden:
                    await SendForbiddenAsync(ct);
                    return;
                default:
                    if (expenses.Errors.Any())
                        AddError(expenses.Errors.First());
                    await SendErrorsAsync(cancellation: ct);
                    return;
            }
        }

        Response = new ListExpensesByIncomeIdResponse
        {
            Transactions = expenses.Value
                .Select(
                    i =>
                        new FinancialTransactionRecord(
                            i.Id,
                            i.Name,
                            i.Amount,
                            i.OperationDate,
                            i.IsMonthly,
                            i.Type
                        )
                )
                .ToList(),
        };
    }
}

