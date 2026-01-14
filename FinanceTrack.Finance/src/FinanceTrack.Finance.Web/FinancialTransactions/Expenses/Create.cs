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

        if (await this.SendResultIfNotOk(result, cancellationToken))
            return;

        Response = new CreateExpenseTransactionResponse(result.Value);
    }
}

