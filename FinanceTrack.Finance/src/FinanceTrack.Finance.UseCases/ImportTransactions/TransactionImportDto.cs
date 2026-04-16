using FinanceTrack.Finance.Core.FinancialTransactionAggregate;

namespace FinanceTrack.Finance.UseCases.ImportTransactions;

public record TransactionImportDto(
    string Name,
    string? Description,
    FinancialTransactionType TransactionType,
    decimal Amount,
    DateOnly OperationDate
);
