using FinanceTrack.Finance.UseCases.FinancialTransactions.Delete;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.Transactions.Incomes;

public class Delete(IMediator mediator) : Endpoint<DeleteIncomeTransactionRequest>
{
    public override void Configure()
    {
        Delete(DeleteIncomeTransactionRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(DeleteIncomeTransactionRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var command = new DeleteIncomeFinancialTransactionCommand(
            TransactionId: req.TransactionId,
            UserId: userId
        );

        var result = await mediator.Send(command, ct);

        if (await this.SendResultIfNotOk(result, ct))
            return;

        await SendNoContentAsync(ct);
    }
}
