namespace FinanceTrack.Finance.Core.TransactionAggregate;

public sealed class Transaction : EntityBase<Guid>, IAggregateRoot
{
    // Keycloak sub is string
    public string UserId { get; private set; } = default!;
    public string Name { get; private set; } = default!;

    // For Income - null, For Expense - reference to parent Income Transaction
    public Guid? IncomeTransactionId { get; private set; }

    public TransactionType TransactionType { get; private set; } = default!;

    public decimal Amount { get; private set; }

    public DateOnly OperationDate { get; private set; }

    public bool IsMonthly { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public Transaction? IncomeTransaction { get; private set; }

    // ORM
    private Transaction() { }

    private Transaction(
        string userId,
        string name,
        TransactionType type,
        decimal amount,
        DateOnly operationDate,
        bool isMonthly,
        Guid? incomeTransactionId,
        DateTime? createdAtUtc = null
    )
    {
        Guard.Against.NullOrWhiteSpace(userId);
        Guard.Against.NullOrWhiteSpace(name);
        // Validate original amount to reject values like 0.009 before rounding
        Guard.Against.OutOfRange(amount, nameof(amount), 0.01m, decimal.MaxValue);
        var roundedAmount = decimal.Round(amount, 2);
        Guard.Against.Default(operationDate);

        if (type == TransactionType.Income && incomeTransactionId.HasValue)
            throw new InvalidOperationException(
                "Income transaction cannot have IncomeTransactionId."
            );

        if (type == TransactionType.Expense && incomeTransactionId is null)
            throw new InvalidOperationException(
                "Expense transaction must have IncomeTransactionId."
            );

        UserId = userId;
        Name = name;
        TransactionType = type;
        Amount = roundedAmount;
        OperationDate = operationDate;
        IsMonthly = isMonthly;
        IncomeTransactionId = incomeTransactionId;
        CreatedAtUtc = createdAtUtc ?? DateTime.UtcNow;
    }

    public static Transaction CreateIncome(
        string userId,
        string name,
        decimal amount,
        DateOnly operationDate,
        bool isMonthly
    ) =>
        new(
            userId: userId,
            name: name,
            type: TransactionType.Income,
            amount: amount,
            operationDate: operationDate,
            isMonthly: isMonthly,
            incomeTransactionId: null
        );

    public static Transaction CreateExpense(
        string userId,
        string name,
        decimal amount,
        DateOnly operationDate,
        bool isMonthly,
        Guid incomeTransactionId
    ) =>
        new(
            userId: userId,
            name: name,
            type: TransactionType.Expense,
            amount: amount,
            operationDate: operationDate,
            isMonthly: isMonthly,
            incomeTransactionId: incomeTransactionId
        );

    public Transaction UpdateName(string name)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Name = name;
        return this;
    }

    public Transaction UpdateAmount(decimal newAmount)
    {
        var roundedNewAmount = decimal.Round(newAmount, 2);
        Guard.Against.NegativeOrZero(roundedNewAmount);
        Amount = roundedNewAmount;
        return this;
    }

    public Transaction SetMonthly(bool isMonthly)
    {
        IsMonthly = isMonthly;
        return this;
    }

    public Transaction SetOperationDate(DateOnly newDate)
    {
        Guard.Against.Default(newDate);
        OperationDate = newDate;
        return this;
    }
}
