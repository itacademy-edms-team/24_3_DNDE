using FinanceTrack.Finance.Core.Interfaces;

namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Income;

public sealed record CreateIncomeCommand(
    string UserId,
    Guid WalletId,
    string Name,
    string? Description,
    decimal Amount,
    DateOnly OperationDate,
    Guid? CategoryId = null,
    Guid? RecurringTransactionId = null
)
    : CreateIncomeRequest(
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
