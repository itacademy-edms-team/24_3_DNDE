using FinanceTrack.Finance.UseCases.FinancialTransactions.Incomes.Update;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.Transactions.Incomes;

public class Update(IMediator _mediator)
    : Endpoint<UpdateIncomeTransactionRequest, FinancialTransactionRecord>
{
    public override void Configure()
    {
        Put(UpdateIncomeTransactionRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(
        UpdateIncomeTransactionRequest request,
        CancellationToken ct
    )
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var command = new UpdateIncomeFinancialTransactionCommand(
            TransactionId: request.TransactionId,
            UserId: userId,
            Name: request.Name,
            Amount: request.Amount,
            OperationDate: request.OperationDate,
            IsMonthly: request.IsMonthly
        );

        var result = await _mediator.Send(command, ct);

        if (await this.SendResultIfNotOk(result, ct))
            return;

        var dto = result.Value;
        Response = new FinancialTransactionRecord(
            dto.Id,
            dto.Name,
            dto.Amount,
            dto.OperationDate,
            dto.IsMonthly,
            dto.Type
        );
    }
}
