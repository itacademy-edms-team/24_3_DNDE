using Ardalis.GuardClauses;
using Ardalis.SharedKernel;

namespace FinanceTrack.Finance.Core.TransactionAggregate;

public sealed class Transaction : EntityBase<Guid>, IAggregateRoot
{
  // Keycloak sub is string
  public string UserId { get; private set; } = default!;

  // For Income - null, For Expense - reference to parent Income Transaction
  public Guid? IncomeTransactionId { get; private set; }

  public TransactionType TransactionType { get; private set; } = default!;

  public decimal Amount { get; private set; }

  public DateOnly OperationDate { get; private set; }

  public bool IsMonthly { get; private set; }

  public DateTime CreatedAtUtc { get; private set; }

  public Transaction? IncomeTransaction { get; private set; }

  // Для ORM
  private Transaction() { }

  private Transaction(
    string userId,
    TransactionType type,
    decimal amount,
    DateOnly operationDate,
    bool isMonthly,
    Guid? incomeTransactionId,
    DateTime? createdAtUtc = null
  )
  {
    Guard.Against.NullOrWhiteSpace(userId);
    Guard.Against.NegativeOrZero(amount);
    Guard.Against.Default(operationDate);

    if (type == TransactionType.Income && incomeTransactionId.HasValue)
      throw new InvalidOperationException("Income transaction cannot have IncomeTransactionId.");

    if (type == TransactionType.Expense && incomeTransactionId is null)
      throw new InvalidOperationException("Expense transaction must have IncomeTransactionId.");

    UserId = userId;
    TransactionType = type;
    Amount = decimal.Round(amount, 2);
    OperationDate = operationDate;
    IsMonthly = isMonthly;
    IncomeTransactionId = incomeTransactionId;
    CreatedAtUtc = createdAtUtc ?? DateTime.UtcNow;
  }

  public static Transaction CreateIncome(
    string userId,
    decimal amount,
    DateOnly operationDate,
    bool isMonthly
  ) =>
    new(
      userId: userId,
      type: TransactionType.Income,
      amount: amount,
      operationDate: operationDate,
      isMonthly: isMonthly,
      incomeTransactionId: null
    );

  public static Transaction CreateExpense(
    string userId,
    decimal amount,
    DateOnly operationDate,
    bool isMonthly,
    Guid incomeTransactionId
  ) =>
    new(
      userId: userId,
      type: TransactionType.Expense,
      amount: amount,
      operationDate: operationDate,
      isMonthly: isMonthly,
      incomeTransactionId: incomeTransactionId
    );

  public Transaction UpdateAmount(decimal newAmount)
  {
    Guard.Against.NegativeOrZero(newAmount);
    Amount = decimal.Round(newAmount, 2);
    return this;
  }

  public Transaction SetMonthly(bool isMonthly)
  {
    IsMonthly = isMonthly;
    return this;
  }
}
