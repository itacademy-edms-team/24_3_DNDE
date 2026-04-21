namespace FinanceTrack.Finance.UseCases.FinancialTransactions.List;

public sealed record ListWalletTransactionsQuery(
    string UserId,
    Guid WalletId,
    DateOnly From,
    DateOnly To,
    string? AfterCursor,
    int PageSize
) : IQuery<Result<FinancialTransactionPageDto>>;
