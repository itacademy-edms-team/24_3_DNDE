using FinanceTrack.Finance.UseCases.RecurringTransactions.ToggleActive;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.RecurringTransactions;

public class ToggleRecurringTransactionRequest
{
    public const string Route = "/RecurringTransactions/{RecurringId:guid}/toggle";
    public Guid RecurringId { get; set; }
    public bool IsActive { get; set; }
}

public class ToggleRecurringTransaction(IMediator mediator)
    : Endpoint<ToggleRecurringTransactionRequest>
{
    public override void Configure()
    {
        Post(ToggleRecurringTransactionRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(ToggleRecurringTransactionRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var command = new ToggleRecurringTransactionCommand(req.RecurringId, userId, req.IsActive);
        var result = await mediator.Send(command, ct);

        if (await this.SendResultIfNotOk(result, ct))
            return;

        await SendNoContentAsync(ct);
    }
}
