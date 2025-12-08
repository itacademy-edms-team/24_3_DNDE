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

        switch (result.Status)
        {
            case ResultStatus.NotFound:
                await SendNotFoundAsync(ct);
                return;

            case ResultStatus.Forbidden:
                await SendForbiddenAsync(ct);
                return;

            case ResultStatus.Invalid:
            case ResultStatus.Error:
                if (result.Errors.Any())
                    AddError(result.Errors.First());
                await SendErrorsAsync(cancellation: ct);
                return;

            case ResultStatus.Ok:
            default:
                var dto = result.Value;
                Response = new FinancialTransactionRecord(
                    dto.Id,
                    dto.Name,
                    dto.Amount,
                    dto.OperationDate,
                    dto.IsMonthly,
                    dto.Type
                );
                return;
        }
    }
}
