using System.Security.Claims;
using FinanceTrack.Finance.UseCases.Transactions.Delete;
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

    var command = new DeleteIncomeTransactionCommand(
      TransactionId: req.TransactionId,
      UserId: userId
    );

    var result = await mediator.Send(command, ct);

    switch (result.Status)
    {
      case ResultStatus.NotFound:
        await SendNotFoundAsync(ct);
        return;

      case ResultStatus.Forbidden:
        await SendForbiddenAsync(ct);
        return;

      case ResultStatus.Error:
      case ResultStatus.Invalid:
        if (result.Errors.Any())
          AddError(result.Errors.First());
        await SendErrorsAsync(cancellation: ct);
        return;

      case ResultStatus.Ok:
      default:
        await SendNoContentAsync(ct);
        return;
    }
  }
}
