namespace FinanceTrack.Finance.UseCases.FinancialTransactions.List;

public sealed record FinancialTransactionPageDto(
    IReadOnlyList<FinancialTransactionDto> Transactions,
    string? NextCursor,
    bool HasMore
);
