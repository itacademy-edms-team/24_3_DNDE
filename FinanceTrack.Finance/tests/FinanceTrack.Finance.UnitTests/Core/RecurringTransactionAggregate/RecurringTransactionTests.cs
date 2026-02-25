using FinanceTrack.Finance.Core.RecurringTransactionAggregate;

namespace FinanceTrack.Finance.UnitTests.Core.RecurringTransactionAggregate;

public class RecurringTransactionTests
{
    private const string UserId = "user-1";
    private static readonly Guid WalletId = Guid.NewGuid();
    private static readonly DateOnly StartDate = new(2026, 1, 1);

    [Fact]
    public void Create_SetsExpectedFields()
    {
        var rt = RecurringTransaction.Create(
            UserId, WalletId, "Monthly Salary",
            RecurringTransactionType.Income, 5000m, 15, StartDate);

        rt.UserId.ShouldBe(UserId);
        rt.WalletId.ShouldBe(WalletId);
        rt.Name.ShouldBe("Monthly Salary");
        rt.TransactionType.ShouldBe(RecurringTransactionType.Income);
        rt.Amount.ShouldBe(5000m);
        rt.DayOfMonth.ShouldBe(15);
        rt.StartDate.ShouldBe(StartDate);
        rt.EndDate.ShouldBeNull();
        rt.IsActive.ShouldBeTrue();
        rt.LastProcessedDate.ShouldBeNull();
        rt.CategoryId.ShouldBeNull();
    }

    [Fact]
    public void Create_WithEndDate_SetsEndDate()
    {
        var endDate = StartDate.AddYears(1);
        var rt = RecurringTransaction.Create(
            UserId, WalletId, "Rent",
            RecurringTransactionType.Expense, 1200m, 1, StartDate, endDate);

        rt.EndDate.ShouldBe(endDate);
    }

    [Fact]
    public void Create_EndDateBeforeStart_Throws()
    {
        var endDate = StartDate.AddDays(-1);

        Should.Throw<ArgumentException>(() =>
            RecurringTransaction.Create(
                UserId, WalletId, "Test",
                RecurringTransactionType.Income, 100m, 1, StartDate, endDate));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(29)]
    [InlineData(-1)]
    public void Create_InvalidDayOfMonth_Throws(int badDay)
    {
        Should.Throw<ArgumentOutOfRangeException>(() =>
            RecurringTransaction.Create(
                UserId, WalletId, "Test",
                RecurringTransactionType.Income, 100m, badDay, StartDate));
    }

    [Fact]
    public void Deactivate_SetsInactive()
    {
        var rt = RecurringTransaction.Create(
            UserId, WalletId, "Test",
            RecurringTransactionType.Income, 100m, 1, StartDate);

        rt.Deactivate();

        rt.IsActive.ShouldBeFalse();
    }

    [Fact]
    public void Activate_ReActivates()
    {
        var rt = RecurringTransaction.Create(
            UserId, WalletId, "Test",
            RecurringTransactionType.Income, 100m, 1, StartDate);
        rt.Deactivate();

        rt.Activate();

        rt.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void MarkProcessed_SetsDate()
    {
        var rt = RecurringTransaction.Create(
            UserId, WalletId, "Test",
            RecurringTransactionType.Income, 100m, 1, StartDate);
        var processedDate = new DateOnly(2026, 2, 1);

        rt.MarkProcessed(processedDate);

        rt.LastProcessedDate.ShouldBe(processedDate);
    }

    [Fact]
    public void UpdateName_SetsNewName()
    {
        var rt = RecurringTransaction.Create(
            UserId, WalletId, "Old Name",
            RecurringTransactionType.Income, 100m, 1, StartDate);

        rt.UpdateName("New Name");

        rt.Name.ShouldBe("New Name");
    }

    [Fact]
    public void UpdateAmount_RoundsAndSets()
    {
        var rt = RecurringTransaction.Create(
            UserId, WalletId, "Test",
            RecurringTransactionType.Income, 100m, 1, StartDate);

        rt.UpdateAmount(123.456m);

        rt.Amount.ShouldBe(123.46m);
    }

    [Fact]
    public void SetDayOfMonth_Updates()
    {
        var rt = RecurringTransaction.Create(
            UserId, WalletId, "Test",
            RecurringTransactionType.Income, 100m, 1, StartDate);

        rt.SetDayOfMonth(15);

        rt.DayOfMonth.ShouldBe(15);
    }

    [Fact]
    public void SetEndDate_BeforeStartDate_Throws()
    {
        var rt = RecurringTransaction.Create(
            UserId, WalletId, "Test",
            RecurringTransactionType.Income, 100m, 1, StartDate);

        Should.Throw<ArgumentException>(() => rt.SetEndDate(StartDate.AddDays(-1)));
    }

    [Fact]
    public void SetCategory_SetsId()
    {
        var rt = RecurringTransaction.Create(
            UserId, WalletId, "Test",
            RecurringTransactionType.Income, 100m, 1, StartDate);
        var catId = Guid.NewGuid();

        rt.SetCategory(catId);

        rt.CategoryId.ShouldBe(catId);
    }
}
