using FinanceTrack.Finance.Core.FinancialTransactionAggregate;

namespace FinanceTrack.Finance.Core.Interfaces;

public interface IUpdateIncomeFinancialTransactionService
{
    Task<Result<FinancialTransaction>> UpdateIncome(
        UpdateIncomeFinancialTransactionRequest request,
        CancellationToken cancellationToken = default
    );
}
