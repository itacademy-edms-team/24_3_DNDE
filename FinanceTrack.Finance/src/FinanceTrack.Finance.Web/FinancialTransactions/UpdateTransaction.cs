using FinanceTrack.Finance.UseCases.FinancialTransactions.Update;
using FinanceTrack.Finance.Web.Extensions;
using FluentValidation;

namespace FinanceTrack.Finance.Web.Transactions;

public class UpdateTransactionRequest
{
    public const string Route = "/Transactions/{TransactionId:guid}";
    public Guid TransactionId { get; set; }
    public string Name { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateOnly OperationDate { get; set; }
    public Guid? CategoryId { get; set; }
}

public class UpdateTransactionValidator : Validator<UpdateTransactionRequest>
{
    public UpdateTransactionValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0.01m)
            .WithMessage("Amount must be at least 0.01.");
        RuleFor(x => x.OperationDate)
            .Must(d => d != default)
            .WithMessage("OperationDate is required.");
    }
}

public class UpdateTransaction(IMediator mediator)
    : Endpoint<UpdateTransactionRequest, FinancialTransactionRecord>
{
    public override void Configure()
    {
        Put(UpdateTransactionRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(UpdateTransactionRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var command = new UpdateTransactionCommand(
            req.TransactionId,
            userId,
            req.Name,
            req.Amount,
            req.OperationDate,
            req.CategoryId
        );
        var result = await mediator.Send(command, ct);

        if (await this.SendResultIfNotOk(result, ct))
            return;

        var dto = result.Value;
        Response = new FinancialTransactionRecord(
            dto.Id,
            dto.WalletId,
            dto.Name,
            dto.Amount,
            dto.OperationDate,
            dto.Type,
            dto.CategoryId,
            dto.RelatedTransactionId,
            dto.RecurringTransactionId,
            dto.RelatedWalletId,
            dto.RelatedWalletName
        );
    }
}
