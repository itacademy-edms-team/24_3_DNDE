using FinanceTrack.Finance.Infrastructure.Data.Config;
using FinanceTrack.Finance.UseCases.FinancialTransactions.Income;
using FinanceTrack.Finance.Web.Extensions;
using FluentValidation;

namespace FinanceTrack.Finance.Web.Transactions;

public class CreateIncomeRequest
{
    public const string Route = "/Transactions/Income";
    public Guid WalletId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateOnly OperationDate { get; set; }
    public Guid? CategoryId { get; set; }
}

public class CreateIncomeResponse(Guid id)
{
    public Guid Id { get; set; } = id;
}

public class CreateIncomeValidator : Validator<CreateIncomeRequest>
{
    public CreateIncomeValidator()
    {
        RuleFor(x => x.WalletId).Must(id => id != Guid.Empty).WithMessage("WalletId is required.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Description)
            .MaximumLength(FinancialTransactionDataSchemaConstants.TransactionDescriptionMaxLength)
            .WithMessage("Description length must be less than 501");
        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0.01m)
            .WithMessage("Amount must be at least 0.01.");
        RuleFor(x => x.OperationDate)
            .Must(d => d != default)
            .WithMessage("OperationDate is required.");
    }
}

public class CreateIncome(IMediator mediator) : Endpoint<CreateIncomeRequest, CreateIncomeResponse>
{
    public override void Configure()
    {
        Post(CreateIncomeRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(CreateIncomeRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var command = new CreateIncomeCommand(
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

        await SendAsync(new CreateIncomeResponse(result.Value), 201, ct);
    }
}
