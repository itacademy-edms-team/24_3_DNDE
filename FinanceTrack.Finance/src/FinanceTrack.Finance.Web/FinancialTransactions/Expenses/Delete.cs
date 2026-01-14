using FinanceTrack.Finance.UseCases.FinancialTransactions.Expenses.Delete;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.Transactions.Expenses;

public class Delete(IMediator mediator) : Endpoint<DeleteExpenseTransactionRequest>
{
    public override void Configure()
    {
        Delete(DeleteExpenseTransactionRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(
        DeleteExpenseTransactionRequest req,
        CancellationToken ct
    )
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var command = new DeleteExpenseFinancialTransactionCommand(
            TransactionId: req.TransactionId,
            UserId: userId
        );

        var result = await mediator.Send(command, ct);

        if (await this.SendResultIfNotOk(result, ct))
            return;

        await SendNoContentAsync(ct);
    }
}
