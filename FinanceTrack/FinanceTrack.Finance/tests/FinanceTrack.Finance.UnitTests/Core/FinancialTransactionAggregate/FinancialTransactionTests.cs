using FinanceTrack.Finance.Core.FinancialTransactionAggregate;

namespace FinanceTrack.Finance.UnitTests.Core.FinancialTransactionAggregate;

public class FinancialTransactionTests
{
    private const string UserId = "user-1";
    private static readonly Guid WalletId = Guid.NewGuid();
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    [Fact]
    public void CreateIncome_SetsExpectedFields()
    {
        var tx = FinancialTransaction.CreateIncome(UserId, WalletId, "Salary", 5000m, Today);

        tx.UserId.ShouldBe(UserId);
        tx.WalletId.ShouldBe(WalletId);
        tx.Name.ShouldBe("Salary");
        tx.TransactionType.ShouldBe(FinancialTransactionType.Income);
        tx.Amount.ShouldBe(5000m);
        tx.OperationDate.ShouldBe(Today);
        tx.CategoryId.ShouldBeNull();
        tx.RelatedTransactionId.ShouldBeNull();
        tx.RecurringTransactionId.ShouldBeNull();
    }

    [Fact]
    public void CreateIncome_WithCategory_SetsCategoryId()
    {
        var catId = Guid.NewGuid();
        var tx = FinancialTransaction.CreateIncome(UserId, WalletId, "Salary", 5000m, Today, catId);

        tx.CategoryId.ShouldBe(catId);
    }

    [Fact]
    public void CreateExpense_SetsExpenseType()
    {
        var tx = FinancialTransaction.CreateExpense(UserId, WalletId, "Rent", 1200m, Today);

        tx.TransactionType.ShouldBe(FinancialTransactionType.Expense);
    }

    [Fact]
    public void CreateTransferOut_SetsTransferOutType()
    {
        var relatedId = Guid.NewGuid();
        var tx = FinancialTransaction.CreateTransferOut(UserId, WalletId, "Transfer", 100m, Today, relatedId);

        tx.TransactionType.ShouldBe(FinancialTransactionType.TransferOut);
        tx.RelatedTransactionId.ShouldBe(relatedId);
    }

    [Fact]
    public void CreateTransferIn_SetsTransferInType()
    {
        var relatedId = Guid.NewGuid();
        var tx = FinancialTransaction.CreateTransferIn(UserId, WalletId, "Transfer", 100m, Today, relatedId);

        tx.TransactionType.ShouldBe(FinancialTransactionType.TransferIn);
        tx.RelatedTransactionId.ShouldBe(relatedId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void CreateIncome_InvalidAmount_Throws(decimal badAmount)
    {
        Should.Throw<ArgumentException>(() =>
            FinancialTransaction.CreateIncome(UserId, WalletId, "Test", badAmount, Today));
    }

    [Fact]
    public void CreateIncome_DefaultDate_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            FinancialTransaction.CreateIncome(UserId, WalletId, "Test", 100m, default));
    }

    [Fact]
    public void CreateIncome_EmptyWalletId_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            FinancialTransaction.CreateIncome(UserId, Guid.Empty, "Test", 100m, Today));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateIncome_InvalidName_Throws(string? badName)
    {
        Should.Throw<ArgumentException>(() =>
            FinancialTransaction.CreateIncome(UserId, WalletId, badName!, 100m, Today));
    }

    [Fact]
    public void CreateIncome_RoundsAmountToTwoDecimals()
    {
        var tx = FinancialTransaction.CreateIncome(UserId, WalletId, "Test", 123.456m, Today);

        tx.Amount.ShouldBe(123.46m);
    }

    [Fact]
    public void UpdateName_SetsNewName()
    {
        var tx = FinancialTransaction.CreateIncome(UserId, WalletId, "Old", 100m, Today);

        tx.UpdateName("New");

        tx.Name.ShouldBe("New");
    }

    [Fact]
    public void UpdateAmount_SetsAndRounds()
    {
        var tx = FinancialTransaction.CreateIncome(UserId, WalletId, "Test", 100m, Today);

        tx.UpdateAmount(55.999m);

        tx.Amount.ShouldBe(56.00m);
    }

    [Fact]
    public void SetOperationDate_Updates()
    {
        var tx = FinancialTransaction.CreateIncome(UserId, WalletId, "Test", 100m, Today);
        var newDate = Today.AddDays(5);

        tx.SetOperationDate(newDate);

        tx.OperationDate.ShouldBe(newDate);
    }

    [Fact]
    public void SetCategory_SetsNewCategoryId()
    {
        var tx = FinancialTransaction.CreateIncome(UserId, WalletId, "Test", 100m, Today);
        var catId = Guid.NewGuid();

        tx.SetCategory(catId);

        tx.CategoryId.ShouldBe(catId);
    }

    [Fact]
    public void SetCategory_Null_ClearsCategory()
    {
        var tx = FinancialTransaction.CreateIncome(UserId, WalletId, "Test", 100m, Today, Guid.NewGuid());

        tx.SetCategory(null);

        tx.CategoryId.ShouldBeNull();
    }
}
