namespace FinanceTrack.Finance.Core.Interfaces;

public interface ICreateExpenseFinancialTransactionService
{
    public Task<Result<Guid>> CreateExpenseFinancialTransaction(
        CreateExpenseFinancialTransactionRequest request,
        CancellationToken cancellationToken = default
    );
}
