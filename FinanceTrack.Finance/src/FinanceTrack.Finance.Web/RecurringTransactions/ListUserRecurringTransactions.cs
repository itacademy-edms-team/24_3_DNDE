using FinanceTrack.Finance.UseCases.RecurringTransactions.List;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.RecurringTransactions;

public class ListRecurringTransactionsResponse
{
    public List<RecurringTransactionRecord> RecurringTransactions { get; set; } = [];
}

public class ListUserRecurringTransactions(IMediator mediator)
    : EndpointWithoutRequest<ListRecurringTransactionsResponse>
{
    public override void Configure()
    {
        Get("/RecurringTransactions");
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

        var items = await mediator.Send(new ListUserRecurringTransactionsQuery(userId), ct);

        Response = new ListRecurringTransactionsResponse
        {
            RecurringTransactions = items
                .Select(r => new RecurringTransactionRecord(
                    r.Id, r.WalletId, r.CategoryId, r.Name, r.Type,
                    r.Amount, r.DayOfMonth, r.StartDate, r.EndDate,
                    r.IsActive, r.LastProcessedDate
                ))
                .ToList(),
        };
    }
}
