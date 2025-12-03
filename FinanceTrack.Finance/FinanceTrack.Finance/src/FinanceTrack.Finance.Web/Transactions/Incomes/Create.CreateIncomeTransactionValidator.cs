using FinanceTrack.Finance.Web.Contributors;
using FluentValidation;

namespace FinanceTrack.Finance.Web.Transactions.Incomes;

public class CreateIncomeTransactionValidator : Validator<CreateIncomeTransactionRequest> // Called automatically when CreateIncomeTransactionRequest is discovered
{
  public CreateIncomeTransactionValidator()
  {
    RuleFor(x => x.Name).NotEmpty().WithMessage("Name must be not empty.");
    RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be greater than zero.");
    RuleFor(x => x.OperationDate).Must(d => d != default).WithMessage("OperationDate is required.");
  }
}
