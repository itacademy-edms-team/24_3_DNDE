using FluentValidation;

namespace FinanceTrack.Finance.Web.Transactions.Expenses;

public class UpdateExpenseTransactionValidator : Validator<UpdateExpenseTransactionRequest>
{
    public UpdateExpenseTransactionValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name must be not empty.");
        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0.01m)
            .WithMessage("Amount must be at least 0.01.");
        RuleFor(x => x.OperationDate)
            .Must(d => d != default)
            .WithMessage("OperationDate is required.");
        RuleFor(x => x.IncomeTransactionId)
            .Must(id => id != Guid.Empty)
            .WithMessage("IncomeTransactionId is required.");
    }
}

