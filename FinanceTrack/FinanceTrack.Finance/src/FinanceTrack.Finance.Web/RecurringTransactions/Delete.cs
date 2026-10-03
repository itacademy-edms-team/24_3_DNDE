using FinanceTrack.Finance.UseCases.RecurringTransactions.Delete;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.RecurringTransactions;

public class DeleteRecurringTransactionRequest
{
    public const string Route = "/RecurringTransactions/{RecurringId:guid}";
    public Guid RecurringId { get; set; }
}

public class DeleteRecurringTransaction(IMediator mediator)
    : Endpoint<DeleteRecurringTransactionRequest>
{
    public override void Configure()
    {
        Delete(DeleteRecurringTransactionRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(DeleteRecurringTransactionRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var result = await mediator.Send(
            new DeleteRecurringTransactionCommand(req.RecurringId, userId),
            ct
        );

        if (await this.SendResultIfNotOk(result, ct))
            return;

        await SendNoContentAsync(ct);
    }
}
