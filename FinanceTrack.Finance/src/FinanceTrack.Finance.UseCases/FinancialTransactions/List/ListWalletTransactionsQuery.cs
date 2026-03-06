namespace FinanceTrack.Finance.UseCases.FinancialTransactions.List;

public sealed record ListWalletTransactionsQuery(
    string UserId,
    Guid WalletId,
    string? Type,
    DateOnly? From,
    DateOnly? To
) : IQuery<Result<IReadOnlyList<FinancialTransactionDto>>>;
