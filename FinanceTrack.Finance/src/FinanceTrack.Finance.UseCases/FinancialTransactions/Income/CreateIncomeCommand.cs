using FinanceTrack.Finance.Core.Interfaces;

namespace FinanceTrack.Finance.UseCases.FinancialTransactions.Income;

public sealed record CreateIncomeCommand(
    string UserId,
    Guid WalletId,
    string Name,
    decimal Amount,
    DateOnly OperationDate,
    Guid? CategoryId = null,
    Guid? RecurringTransactionId = null
)
    : CreateIncomeRequest(
        UserId,
        WalletId,
        Name,
        Amount,
        OperationDate,
        CategoryId,
        RecurringTransactionId
    ),
        ICommand<Result<Guid>>;
