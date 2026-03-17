using FinanceTrack.Finance.Infrastructure.Data.Config;
using FinanceTrack.Finance.UseCases.RecurringTransactions.Create;
using FinanceTrack.Finance.Web.Extensions;
using FluentValidation;

namespace FinanceTrack.Finance.Web.RecurringTransactions;

public class CreateRecurringTransactionRequest
{
    public const string Route = "/RecurringTransactions";
    public Guid WalletId { get; set; }
    public Guid? CategoryId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string Type { get; set; } = null!; // "Income" or "Expense"
    public decimal Amount { get; set; }
    public int DayOfMonth { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}

public class CreateRecurringTransactionResponse(Guid id)
{
    public Guid Id { get; set; } = id;
}

public class CreateRecurringTransactionValidator : Validator<CreateRecurringTransactionRequest>
{
    public CreateRecurringTransactionValidator()
    {
        RuleFor(x => x.WalletId).Must(id => id != Guid.Empty).WithMessage("WalletId is required.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Description)
            .MaximumLength(
                FinancialTransactionDataSchemaConstants.TRANSACTION_DESCRIPTION_MAX_LENGTH
            )
            .WithMessage("Description length must be less than 501");
        RuleFor(x => x.Type).NotEmpty().WithMessage("Type is required.");
        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0.01m)
            .WithMessage("Amount must be at least 0.01.");
        RuleFor(x => x.DayOfMonth)
            .InclusiveBetween(1, 28)
            .WithMessage("DayOfMonth must be between 1 and 28.");
        RuleFor(x => x.StartDate).Must(d => d != default).WithMessage("StartDate is required.");
    }
}

public class CreateRecurringTransaction(IMediator mediator)
    : Endpoint<CreateRecurringTransactionRequest, CreateRecurringTransactionResponse>
{
    public override void Configure()
    {
        Post(CreateRecurringTransactionRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(
        CreateRecurringTransactionRequest req,
        CancellationToken ct
    )
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var command = new CreateRecurringTransactionCommand(
            userId,
            req.WalletId,
            req.CategoryId,
            req.Name,
            req.Description,
            req.Type,
            req.Amount,
            req.DayOfMonth,
            req.StartDate,
            req.EndDate
        );
        var result = await mediator.Send(command, ct);

        if (await this.SendResultIfNotOk(result, ct))
            return;

        await SendAsync(new CreateRecurringTransactionResponse(result.Value), 201, ct);
    }
}
