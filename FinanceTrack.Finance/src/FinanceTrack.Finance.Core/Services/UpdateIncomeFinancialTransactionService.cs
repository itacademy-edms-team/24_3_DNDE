using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.FinancialTransactionAggregate.Specifications;
using FinanceTrack.Finance.Core.Interfaces;

namespace FinanceTrack.Finance.Core.Services;

public class UpdateIncomeFinancialTransactionService(IRepository<FinancialTransaction> _repo)
{
    public async Task<Result<FinancialTransaction>> UpdateIncome(
        UpdateIncomeFinancialTransactionRequest request,
        CancellationToken ct = default
    )
    {
        var income = await _repo.GetByIdAsync(request.TransactionId, ct);

        if (income == null)
            return Result.NotFound();

        if (!string.Equals(income.UserId, request.UserId, StringComparison.Ordinal))
            return Result.Forbidden();

        if (income.TransactionType != FinancialTransactionType.Income)
            return Result.Error("Only income transactions can be updated with this operation.");

        // Income.IsMonthly = false -> ChildExpense.IsMonthly = false to all children
        var entitiesToUpdate = new List<FinancialTransaction>();
        if (income.IsMonthly && !request.IsMonthly)
        {
            var expenses = await _repo.ListAsync(
                new FinancialTransactionsByIncomeIdSpec(income.Id),
                ct
            );
            foreach (var expense in expenses)
            {
                expense.SetMonthly(false);
            }
            entitiesToUpdate.AddRange(expenses);
        }

        income
            .UpdateName(request.Name)
            .UpdateAmount(request.Amount)
            .SetOperationDate(request.OperationDate)
            .SetMonthly(request.IsMonthly);
        entitiesToUpdate.Add(income);

        await _repo.UpdateRangeAsync(entitiesToUpdate, ct);

        return Result.Success(income);
    }
}
