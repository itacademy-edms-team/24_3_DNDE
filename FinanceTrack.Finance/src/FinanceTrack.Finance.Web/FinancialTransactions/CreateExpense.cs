using FinanceTrack.Finance.Infrastructure.Data.Config;
using FinanceTrack.Finance.UseCases.FinancialTransactions.Expense;
using FinanceTrack.Finance.Web.Extensions;
using FluentValidation;

namespace FinanceTrack.Finance.Web.Transactions;

public class CreateExpenseRequest
{
    public const string Route = "/Transactions/Expense";
    public Guid WalletId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateOnly OperationDate { get; set; }
    public Guid? CategoryId { get; set; }
}

public class CreateExpenseResponse(Guid id)
{
    public Guid Id { get; set; } = id;
}

public class CreateExpenseValidator : Validator<CreateExpenseRequest>
{
    public CreateExpenseValidator()
    {
        RuleFor(x => x.WalletId).Must(id => id != Guid.Empty).WithMessage("WalletId is required.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Description)
            .MaximumLength(
                FinancialTransactionDataSchemaConstants.TRANSACTION_DESCRIPTION_MAX_LENGTH
            )
            .WithMessage("Description length must be less than 501");
        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0.01m)
            .WithMessage("Amount must be at least 0.01.");
        RuleFor(x => x.OperationDate)
            .Must(d => d != default)
            .WithMessage("OperationDate is required.");
    }
}

public class CreateExpense(IMediator mediator)
    : Endpoint<CreateExpenseRequest, CreateExpenseResponse>
{
    public override void Configure()
    {
        Post(CreateExpenseRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(CreateExpenseRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var command = new CreateExpenseCommand(
            userId,
            req.WalletId,
            req.Name,
            req.Description,
            req.Amount,
            req.OperationDate,
            req.CategoryId
        );
        var result = await mediator.Send(command, ct);

        if (await this.SendResultIfNotOk(result, ct))
            return;

        await SendAsync(new CreateExpenseResponse(result.Value), 201, ct);
    }
}
