using FluentValidation;

namespace FinanceTrack.Finance.Web.Transactions.Incomes;

public class UpdateIncomeTransactionValidator : Validator<UpdateIncomeTransactionRequest>
{
    public UpdateIncomeTransactionValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name must be not empty.");
        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0.01m)
            .WithMessage("Amount must be at least 0.01.");
        RuleFor(x => x.OperationDate)
            .Must(d => d != default)
            .WithMessage("OperationDate is required.");
    }
}
