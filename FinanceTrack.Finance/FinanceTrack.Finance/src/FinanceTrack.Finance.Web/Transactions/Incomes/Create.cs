using FinanceTrack.Finance.UseCases.Transactions.Incomes.Create;
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

    var command = new CreateIncomeTransactionCommand(
      UserId: userId,
      Name: request.Name,
      Amount: request.Amount,
      OperationDate: request.OperationDate,
      IsMonthly: request.IsMonthly
    );

    var result = await mediator.Send(command, cancellationToken);
    if (!result.IsSuccess)
    {
      // TODO: маппинг ошибки в HTTP (400/500 и т.п.)
      await SendErrorsAsync(cancellation: cancellationToken);
      return;
    }
    var id = result.Value;

    Response = new CreateIncomeTransactionResponse(id);
  }
}
