using System.Security.Claims;
using FinanceTrack.Finance.UseCases.Transactions.Incomes.List;
using FinanceTrack.Finance.Web.Extensions;
using MediatR;

namespace FinanceTrack.Finance.Web.Transactions.Incomes;

public class ListUserIncomes(IMediator _mediator)
  : EndpointWithoutRequest<ListIncomeTransactionsByUserIdResponse>
{
  public override void Configure()
  {
    Get(ListUserIncomesByUserIdRequest.Route);
    Roles("user");
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var userId = User.GetUserId();
    if (string.IsNullOrWhiteSpace(userId))
    {
      await SendUnauthorizedAsync(ct);
      return;
    }

    var incomes = await _mediator.Send(new ListUserIncomeTransactionsQuery(userId), ct);

    Response = new ListIncomeTransactionsByUserIdResponse
    {
      Transactions = incomes
        .Select(i => new TransactionRecord(i.Id, i.Amount, i.OperationDate, i.IsMonthly, i.Type))
        .ToList(),
    };
  }
}
