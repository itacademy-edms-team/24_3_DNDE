using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.Services;

namespace FinanceTrack.Finance.UseCases.ImportTransactions;

public class ImportTransactionsService(
    CreateExpenseService expenseService,
    CreateIncomeService incomeService
)
{
    public async Task<Result<int>> ImportAsync(
        List<TransactionImportDto> dtos,
        Guid walletId,
        string userId,
        CancellationToken cancel
    )
    {
        foreach (var dto in dtos)
        {
            Result<Guid> result = dto.TransactionType switch
            {
                var t when t == FinancialTransactionType.Income => await incomeService.Execute(
                    new CreateIncomeRequest(
                        userId,
                        walletId,
                        dto.Name,
                        dto.Description,
                        dto.Amount,
                        dto.OperationDate
                    ),
                    cancel
                ),
                var t when t == FinancialTransactionType.Expense => await expenseService.Execute(
                    new CreateExpenseRequest(
                        userId,
                        walletId,
                        dto.Name,
                        dto.Description,
                        dto.Amount,
                        dto.OperationDate
                    ),
                    cancel
                ),
                _ => Result<Guid>.Error(
                    $"Unsupported transaction type: {dto.TransactionType.Name}."
                ),
            };

            if (!result.IsSuccess)
                return Result<int>.Error(
                    result.Errors.FirstOrDefault() ?? "Failed to import transaction."
                );
        }

        return Result.Success(dtos.Count);
    }
}
