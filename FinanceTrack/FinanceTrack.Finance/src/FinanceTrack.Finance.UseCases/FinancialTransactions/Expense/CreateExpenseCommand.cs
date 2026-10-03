using FinanceTrack.Finance.Core.RecurringTransactionAggregate;

namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Expense;

public sealed record CreateExpenseCommand(
    string UserId,
    Guid WalletId,
    string Name,
    string? Description,
    decimal Amount,
    DateOnly OperationDate,
    Guid? CategoryId = null,
    Guid? RecurringTransactionId = null
)
    : CreateExpenseRequest(
        UserId,
        WalletId,
        Name,
        Description,
        Amount,
        OperationDate,
        CategoryId,
        RecurringTransactionId
    ),
        ICommand<Result<Guid>>;
