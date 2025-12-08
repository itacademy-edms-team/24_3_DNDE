using FinanceTrack.Finance.UseCases.FinancialTransactions.Incomes.Create;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.Transactions.Incomes;

public class Create(IMediator mediator)
    : Endpoint<CreateIncomeTransactionRequest, CreateIncomeTransactionResponse>
{
    public override void Configure()
    {
        Post(CreateIncomeTransactionRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(
        CreateIncomeTransactionRequest request,
        CancellationToken cancellationToken
    )
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(cancellationToken);
            return;
        }

        var command = new CreateIncomeFinancialTransactionCommand(
            UserId: userId,
            Name: request.Name,
            Amount: request.Amount,
            OperationDate: request.OperationDate,
            IsMonthly: request.IsMonthly
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
                Response = new CreateIncomeTransactionResponse(id);
                return;
        }
    }
}
