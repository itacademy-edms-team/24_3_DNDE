using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.Interfaces;

namespace FinanceTrack.Finance.Core.Services;

public class CreateIncomeFinancialTransactionService(IRepository<FinancialTransaction> _repo)
{
    public async Task<Result<Guid>> AddIncome(
        CreateIncomeFinancialTransactionRequest request,
        CancellationToken ct = default
    )
    {
        var income = FinancialTransaction.CreateIncome(
            request.userId,
            request.name,
            request.amount,
            request.operationDate,
            request.isMonthly
        );

        await _repo.AddAsync(income);

        return Result.Success(income.Id);
    }
}
