using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinanceTrack.Finance.Core.TransactionAggregate;
using FinanceTrack.Finance.UseCases.Transactions.CreateIncome;

namespace FinanceTrack.Finance.UseCases.Transactions.CreateExpense;

public sealed class CreateExpenseTransactionHandler(IRepository<Transaction> transactionRepository)
  : ICommandHandler<CreateExpenseTransactionCommand, Result<Guid>>
{
  public async Task<Result<Guid>> Handle(
    CreateExpenseTransactionCommand request,
    CancellationToken cancellationToken
  )
  {
    var expense = Transaction.CreateExpense(
      userId: request.UserId,
      amount: request.Amount,
      operationDate: request.OperationDate,
      isMonthly: request.IsMonthly,
      incomeTransactionId: request.IncomeTransactionId
    );

    await transactionRepository.AddAsync(expense, cancellationToken);

    return expense.Id;
  }
}
