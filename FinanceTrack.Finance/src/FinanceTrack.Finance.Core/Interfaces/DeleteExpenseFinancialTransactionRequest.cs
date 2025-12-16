namespace FinanceTrack.Finance.Core.Interfaces;

public sealed record DeleteExpenseFinancialTransactionRequest(Guid TransactionId, string UserId);
