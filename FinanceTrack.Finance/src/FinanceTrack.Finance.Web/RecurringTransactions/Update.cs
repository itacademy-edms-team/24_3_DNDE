using FinanceTrack.Finance.Infrastructure.Data.Config;
using FinanceTrack.Finance.UseCases.RecurringTransactions.Update;
using FinanceTrack.Finance.Web.Extensions;
using FluentValidation;

namespace FinanceTrack.Finance.Web.RecurringTransactions;

public class UpdateRecurringTransactionRequest
{
    public const string Route = "/RecurringTransactions/{RecurringId:guid}";
    public Guid RecurringId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public int DayOfMonth { get; set; }
    public DateOnly? EndDate { get; set; }
    public Guid? CategoryId { get; set; }
}

public class UpdateRecurringTransactionValidator : Validator<UpdateRecurringTransactionRequest>
{
    public UpdateRecurringTransactionValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Description)
            .MaximumLength(FinancialTransactionDataSchemaConstants.TransactionDescriptionMaxLength)
            .WithMessage("Description length must be less than 501");
        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0.01m)
            .WithMessage("Amount must be at least 0.01.");
        RuleFor(x => x.DayOfMonth)
            .InclusiveBetween(1, 28)
            .WithMessage("DayOfMonth must be between 1 and 28.");
    }
}

public class UpdateRecurringTransaction(IMediator mediator)
    : Endpoint<UpdateRecurringTransactionRequest, RecurringTransactionRecord>
{
    public override void Configure()
    {
        Put(UpdateRecurringTransactionRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(
        UpdateRecurringTransactionRequest req,
        CancellationToken ct
    )
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var command = new UpdateRecurringTransactionCommand(
            req.RecurringId,
            userId,
            req.Name,
            req.Description,
            req.Amount,
            req.DayOfMonth,
            req.EndDate,
            req.CategoryId
        );
        var result = await mediator.Send(command, ct);

        if (await this.SendResultIfNotOk(result, ct))
            return;

        var r = result.Value;
        Response = new RecurringTransactionRecord(
            r.Id,
            r.WalletId,
            r.CategoryId,
            r.Name,
            r.Description,
            r.Type,
            r.Amount,
            r.DayOfMonth,
            r.StartDate,
            r.EndDate,
            r.IsActive,
            r.LastProcessedDate
        );
    }
}
