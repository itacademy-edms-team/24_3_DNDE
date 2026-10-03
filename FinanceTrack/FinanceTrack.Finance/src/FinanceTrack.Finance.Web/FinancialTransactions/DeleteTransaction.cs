using FinanceTrack.Finance.UseCases.FinancialTransactions.Delete;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.Transactions;

public class DeleteTransactionRequest
{
    public const string Route = "/Transactions/{TransactionId:guid}";
    public Guid TransactionId { get; set; }
}

public class DeleteTransaction(IMediator mediator) : Endpoint<DeleteTransactionRequest>
{
    public override void Configure()
    {
        Delete(DeleteTransactionRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(DeleteTransactionRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var result = await mediator.Send(new DeleteTransactionCommand(req.TransactionId, userId), ct);

        if (await this.SendResultIfNotOk(result, ct))
            return;

        await SendNoContentAsync(ct);
    }
}
