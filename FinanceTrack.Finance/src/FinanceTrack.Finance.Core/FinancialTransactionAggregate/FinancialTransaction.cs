using FinanceTrack.Finance.Core.CategoryAggregate;
using FinanceTrack.Finance.Core.Shared;
using FinanceTrack.Finance.Core.WalletAggregate;

namespace FinanceTrack.Finance.Core.FinancialTransactionAggregate;

public sealed class FinancialTransaction : GuidEntityBase, IAggregateRoot
{
    public string UserId { get; private set; } = default!;
    public Guid WalletId { get; private set; }
    public Guid? CategoryId { get; private set; }
    public string Name { get; private set; } = default!;
    public FinancialTransactionType TransactionType { get; private set; } = default!;
    public decimal Amount { get; private set; }
    public DateOnly OperationDate { get; private set; }
    public Guid? RelatedTransactionId { get; private set; }
    public Guid? RecurringTransactionId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    // Navigation properties
    public Wallet Wallet { get; private set; } = default!;
    public Category? Category { get; private set; }

    // ORM
    private FinancialTransaction() { }

    private FinancialTransaction(
        string userId,
        Guid walletId,
        string name,
        FinancialTransactionType type,
        decimal amount,
        DateOnly operationDate,
        Guid? categoryId = null,
        Guid? relatedTransactionId = null,
        Guid? recurringTransactionId = null,
        DateTime? createdAtUtc = null
    )
    {
        Guard.Against.NullOrWhiteSpace(userId);
        Guard.Against.Default(walletId);
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.OutOfRange(amount, nameof(amount), 0.01m, decimal.MaxValue);
        Guard.Against.Default(operationDate);

        UserId = userId;
        WalletId = walletId;
        Name = name;
        TransactionType = type;
        Amount = decimal.Round(amount, 2);
        OperationDate = operationDate;
        CategoryId = categoryId;
        RelatedTransactionId = relatedTransactionId;
        RecurringTransactionId = recurringTransactionId;
        CreatedAtUtc = createdAtUtc ?? DateTime.UtcNow;
    }

    public static FinancialTransaction CreateIncome(
        string userId,
        Guid walletId,
        string name,
        decimal amount,
        DateOnly operationDate,
        Guid? categoryId = null,
        Guid? recurringTransactionId = null
    ) =>
        new(
            userId,
            walletId,
            name,
            FinancialTransactionType.Income,
            amount,
            operationDate,
            categoryId: categoryId,
            recurringTransactionId: recurringTransactionId
        );

    public static FinancialTransaction CreateExpense(
        string userId,
        Guid walletId,
        string name,
        decimal amount,
        DateOnly operationDate,
        Guid? categoryId = null,
        Guid? recurringTransactionId = null
    ) =>
        new(
            userId,
            walletId,
            name,
            FinancialTransactionType.Expense,
            amount,
            operationDate,
            categoryId: categoryId,
            recurringTransactionId: recurringTransactionId
        );

    public static FinancialTransaction CreateTransferOut(
        string userId,
        Guid walletId,
        string name,
        decimal amount,
        DateOnly operationDate,
        Guid relatedTransactionId
    ) =>
        new(
            userId,
            walletId,
            name,
            FinancialTransactionType.TransferOut,
            amount,
            operationDate,
            relatedTransactionId: relatedTransactionId
        );

    public static FinancialTransaction CreateTransferIn(
        string userId,
        Guid walletId,
        string name,
        decimal amount,
        DateOnly operationDate,
        Guid relatedTransactionId
    ) =>
        new(
            userId,
            walletId,
            name,
            FinancialTransactionType.TransferIn,
            amount,
            operationDate,
            relatedTransactionId: relatedTransactionId
        );

    public FinancialTransaction UpdateName(string name)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Name = name;
        return this;
    }

    public FinancialTransaction UpdateAmount(decimal newAmount)
    {
        var roundedNewAmount = decimal.Round(newAmount, 2);
        Guard.Against.NegativeOrZero(roundedNewAmount);
        Amount = roundedNewAmount;
        return this;
    }

    public FinancialTransaction SetOperationDate(DateOnly newDate)
    {
        Guard.Against.Default(newDate);
        OperationDate = newDate;
        return this;
    }

    public FinancialTransaction SetCategory(Guid? categoryId)
    {
        CategoryId = categoryId;
        return this;
    }

    internal void SetRelatedTransactionId(Guid relatedId)
    {
        RelatedTransactionId = relatedId;
    }
}
