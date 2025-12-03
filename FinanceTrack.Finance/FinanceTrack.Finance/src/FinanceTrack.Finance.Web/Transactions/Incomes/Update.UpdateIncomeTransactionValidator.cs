using Ardalis.Specification;
using FluentValidation;

namespace FinanceTrack.Finance.Web.Transactions.Incomes;

public class UpdateIncomeTransactionValidator : Validator<UpdateIncomeTransactionRequest>
{
  public UpdateIncomeTransactionValidator()
  {
    RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be greater than 0.");
    RuleFor(x => x.OperationDate).Must(d => d != default).WithMessage("OperationDate is required.");
  }
}
