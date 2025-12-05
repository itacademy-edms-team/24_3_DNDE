using FinanceTrack.Finance.Core.TransactionAggregate;

namespace FinanceTrack.Finance.UnitTests.Core.TransactionAggregate;

public class TransactionIncomeTests
{
    [Fact]
    public void CreateIncome_SetsExpectedFields()
    {
        var userId = "user-1";
        var name = "Salary";
        var amount = 123.456m;
        var date = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var isMonthly = true;

        var transaction = Transaction.CreateIncome(userId, name, amount, date, isMonthly);

        transaction.UserId.ShouldBe(userId);
        transaction.Name.ShouldBe(name);
        transaction.TransactionType.ShouldBe(TransactionType.Income);
        transaction.IncomeTransactionId.ShouldBeNull();
        transaction.Amount.ShouldBe(123.46m);
        transaction.OperationDate.ShouldBe(date);
        transaction.IsMonthly.ShouldBeTrue();

        transaction.CreatedAtUtc.ShouldBeGreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-1));
        transaction.CreatedAtUtc.ShouldBeLessThanOrEqualTo(DateTime.UtcNow.AddMinutes(1));
    }

    [Theory]
    [InlineData(-10)]
    [InlineData(0)]
    [InlineData(0.009)]
    public void CreateIncome_InvalidAmount_ThrowsArgumentException(decimal badAmount)
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        Should.Throw<ArgumentException>(() =>
            Transaction.CreateIncome("user", "Salary", badAmount, date, false)
        );
    }

    [Fact]
    public void CreateIncome_DefaultDate_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() =>
            Transaction.CreateIncome("user", "Salary", 10, default, false)
        );
    }

    [Fact]
    public void CreateIncome_RoundsAmountUp()
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var transaction = Transaction.CreateIncome("user", "Salary", 10.999m, date, false);

        transaction.Amount.ShouldBe(11.00m);
    }

    [Fact]
    public void UpdateAmount_RoundsAndSets()
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var transaction = Transaction.CreateIncome("user", "Salary", 10m, date, false);

        transaction.UpdateAmount(12.345m);

        transaction.Amount.ShouldBe(12.34m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void UpdateAmount_NonPositive_Throws(decimal badAmount)
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var transaction = Transaction.CreateIncome("user", "Salary", 10m, date, false);

        Should.Throw<ArgumentException>(() => transaction.UpdateAmount(badAmount));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateName_Invalid_Throws(string? badName)
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var transaction = Transaction.CreateIncome("user", "Salary", 10m, date, false);

        Should.Throw<ArgumentException>(() => transaction.UpdateName(badName!));
    }

    [Fact]
    public void SetOperationDate_Default_Throws()
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var transaction = Transaction.CreateIncome("user", "Salary", 10m, date, false);

        Should.Throw<ArgumentException>(() => transaction.SetOperationDate(default));
    }

    [Fact]
    public void SetOperationDate_UpdatesValue()
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var newDate = date.AddDays(1);
        var transaction = Transaction.CreateIncome("user", "Salary", 10m, date, false);

        transaction.SetOperationDate(newDate);

        transaction.OperationDate.ShouldBe(newDate);
    }

    [Fact]
    public void SetMonthly_UpdatesFlag()
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var transaction = Transaction.CreateIncome("user", "Salary", 10m, date, false);

        transaction.SetMonthly(true);

        transaction.IsMonthly.ShouldBeTrue();
    }
}
