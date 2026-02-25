namespace FinanceTrack.Finance.Core.WalletAggregate;

public sealed class Wallet : EntityBase<Guid>, IAggregateRoot
{
    public string UserId { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public WalletType WalletType { get; private set; } = default!;
    public decimal Balance { get; private set; }
    public bool AllowNegativeBalance { get; private set; }
    public decimal? TargetAmount { get; private set; }
    public DateOnly? TargetDate { get; private set; }
    public bool IsArchived { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    // ORM
    private Wallet() { }

    public static Wallet CreateChecking(string userId, string name, bool allowNegativeBalance = true)
    {
        Guard.Against.NullOrWhiteSpace(userId);
        Guard.Against.NullOrWhiteSpace(name);

        return new Wallet
        {
            UserId = userId,
            Name = name,
            WalletType = WalletType.Checking,
            Balance = 0m,
            AllowNegativeBalance = allowNegativeBalance,
            IsArchived = false,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    public static Wallet CreateSavings(
        string userId,
        string name,
        decimal targetAmount,
        DateOnly? targetDate = null,
        bool allowNegativeBalance = false
    )
    {
        Guard.Against.NullOrWhiteSpace(userId);
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.NegativeOrZero(targetAmount);

        return new Wallet
        {
            UserId = userId,
            Name = name,
            WalletType = WalletType.Savings,
            Balance = 0m,
            AllowNegativeBalance = allowNegativeBalance,
            TargetAmount = decimal.Round(targetAmount, 2),
            TargetDate = targetDate,
            IsArchived = false,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    public void Credit(decimal amount)
    {
        Guard.Against.NegativeOrZero(amount);
        Balance += decimal.Round(amount, 2);
    }

    public void Debit(decimal amount)
    {
        Guard.Against.NegativeOrZero(amount);
        var rounded = decimal.Round(amount, 2);

        if (!AllowNegativeBalance && Balance < rounded)
        {
            throw new InvalidOperationException(
                $"Insufficient funds. Balance: {Balance}, requested: {rounded}."
            );
        }

        Balance -= rounded;
    }

    public Wallet UpdateName(string name)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Name = name;
        return this;
    }

    public Wallet SetAllowNegativeBalance(bool allow)
    {
        AllowNegativeBalance = allow;
        return this;
    }

    public Wallet UpdateTarget(decimal? targetAmount, DateOnly? targetDate)
    {
        if (targetAmount.HasValue)
        {
            Guard.Against.NegativeOrZero(targetAmount.Value);
            TargetAmount = decimal.Round(targetAmount.Value, 2);
        }
        else
        {
            TargetAmount = null;
        }

        TargetDate = targetDate;
        return this;
    }

    public void Archive() => IsArchived = true;

    public void Unarchive() => IsArchived = false;
}
