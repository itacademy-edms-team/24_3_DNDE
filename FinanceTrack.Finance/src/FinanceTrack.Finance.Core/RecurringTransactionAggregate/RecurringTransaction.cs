using FinanceTrack.Finance.Core.CategoryAggregate;
using FinanceTrack.Finance.Core.Shared;
using FinanceTrack.Finance.Core.WalletAggregate;

namespace FinanceTrack.Finance.Core.RecurringTransactionAggregate;

public sealed class RecurringTransaction : GuidEntityBase, IAggregateRoot
{
    public string UserId { get; private set; } = default!;
    public Guid WalletId { get; private set; }
    public Guid? CategoryId { get; private set; }
    public string Name { get; private set; } = default!;
    public RecurringTransactionType TransactionType { get; private set; } = default!;
    public decimal Amount { get; private set; }
    public int DayOfMonth { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public bool IsActive { get; private set; }
    public DateOnly? LastProcessedDate { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    // Navigation
    public Wallet? Wallet { get; private set; }
    public Category? Category { get; private set; }

    // ORM
    private RecurringTransaction() { }

    public static RecurringTransaction Create(
        string userId,
        Guid walletId,
        string name,
        RecurringTransactionType type,
        decimal amount,
        int dayOfMonth,
        DateOnly startDate,
        DateOnly? endDate = null,
        Guid? categoryId = null
    )
    {
        Guard.Against.NullOrWhiteSpace(userId);
        Guard.Against.Default(walletId);
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.OutOfRange(amount, nameof(amount), 0.01m, decimal.MaxValue);
        Guard.Against.OutOfRange(dayOfMonth, nameof(dayOfMonth), 1, 28);
        Guard.Against.Default(startDate);

        if (endDate.HasValue && endDate.Value < startDate)
        {
            throw new ArgumentException("EndDate cannot be before StartDate.", nameof(endDate));
        }

        return new RecurringTransaction
        {
            UserId = userId,
            WalletId = walletId,
            CategoryId = categoryId,
            Name = name,
            TransactionType = type,
            Amount = decimal.Round(amount, 2),
            DayOfMonth = dayOfMonth,
            StartDate = startDate,
            EndDate = endDate,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;

    public void MarkProcessed(DateOnly date) => LastProcessedDate = date;

    public RecurringTransaction UpdateName(string name)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Name = name;
        return this;
    }

    public RecurringTransaction UpdateAmount(decimal amount)
    {
        Guard.Against.OutOfRange(amount, nameof(amount), 0.01m, decimal.MaxValue);
        Amount = decimal.Round(amount, 2);
        return this;
    }

    public RecurringTransaction SetDayOfMonth(int day)
    {
        Guard.Against.OutOfRange(day, nameof(day), 1, 28);
        DayOfMonth = day;
        return this;
    }

    public RecurringTransaction SetEndDate(DateOnly? endDate)
    {
        if (endDate.HasValue && endDate.Value < StartDate)
        {
            throw new ArgumentException("EndDate cannot be before StartDate.", nameof(endDate));
        }

        EndDate = endDate;
        return this;
    }

    public RecurringTransaction SetCategory(Guid? categoryId)
    {
        CategoryId = categoryId;
        return this;
    }
}
