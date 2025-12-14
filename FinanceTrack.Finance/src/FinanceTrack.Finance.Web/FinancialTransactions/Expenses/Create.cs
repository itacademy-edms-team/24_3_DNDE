using FinanceTrack.Finance.UseCases.FinancialTransactions.Expenses.Create;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.Transactions.Expenses;

public class Create(IMediator mediator)
    : Endpoint<CreateExpenseTransactionRequest, CreateExpenseTransactionResponse>
{
    public override void Configure()
    {
        Post(CreateExpenseTransactionRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(
        CreateExpenseTransactionRequest request,
        CancellationToken cancellationToken
    )
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(cancellationToken);
            return;
        }

        var command = new CreateExpenseFinancialTransactionCommand(
            UserId: userId,
            Name: request.Name,
            Amount: request.Amount,
            OperationDate: request.OperationDate,
            IsMonthly: request.IsMonthly,
            IncomeTransactionId: request.IncomeTransactionId
        );

        var result = await mediator.Send(command, cancellationToken);

        switch (result.Status)
        {
            case ResultStatus.Forbidden:
                await SendForbiddenAsync(cancellationToken);
                return;

            case ResultStatus.NotFound:
                await SendNotFoundAsync(cancellationToken);
                return;

            case ResultStatus.Error:
            case ResultStatus.Invalid:
                if (result.Errors.Any())
                    AddError(result.Errors.First());
                await SendErrorsAsync(cancellation: cancellationToken);
                return;

            case ResultStatus.Ok:
            default:
                var id = result.Value;
                Response = new CreateExpenseTransactionResponse(id);
                return;
        }
    }
}

