using FinanceTrack.Finance.Core.WalletAggregate;

namespace FinanceTrack.Finance.UnitTests.Core.WalletAggregate;

public class WalletTests
{
    private const string UserId = "user-1";
    private const string WalletName = "Main Account";

    [Fact]
    public void CreateChecking_SetsExpectedFields()
    {
        var wallet = Wallet.CreateChecking(UserId, WalletName);

        wallet.UserId.ShouldBe(UserId);
        wallet.Name.ShouldBe(WalletName);
        wallet.WalletType.ShouldBe(WalletType.Checking);
        wallet.Balance.ShouldBe(0m);
        wallet.AllowNegativeBalance.ShouldBeTrue();
        wallet.IsArchived.ShouldBeFalse();
        wallet.TargetAmount.ShouldBeNull();
        wallet.TargetDate.ShouldBeNull();
    }

    [Fact]
    public void CreateChecking_AllowNegativeFalse_SetsFlag()
    {
        var wallet = Wallet.CreateChecking(UserId, WalletName, allowNegativeBalance: false);

        wallet.AllowNegativeBalance.ShouldBeFalse();
    }

    [Fact]
    public void CreateSavings_SetsExpectedFields()
    {
        var targetDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1));
        var wallet = Wallet.CreateSavings(UserId, "Vacation Fund", 10_000m, targetDate);

        wallet.UserId.ShouldBe(UserId);
        wallet.Name.ShouldBe("Vacation Fund");
        wallet.WalletType.ShouldBe(WalletType.Savings);
        wallet.Balance.ShouldBe(0m);
        wallet.AllowNegativeBalance.ShouldBeFalse();
        wallet.TargetAmount.ShouldBe(10_000m);
        wallet.TargetDate.ShouldBe(targetDate);
        wallet.IsArchived.ShouldBeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateChecking_InvalidUserId_Throws(string? badUserId)
    {
        Should.Throw<ArgumentException>(() =>
            Wallet.CreateChecking(badUserId!, WalletName));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateChecking_InvalidName_Throws(string? badName)
    {
        Should.Throw<ArgumentException>(() =>
            Wallet.CreateChecking(UserId, badName!));
    }

    [Fact]
    public void Credit_IncreasesBalance()
    {
        var wallet = Wallet.CreateChecking(UserId, WalletName);

        wallet.Credit(100m);

        wallet.Balance.ShouldBe(100m);
    }

    [Fact]
    public void Credit_MultipleTimes_Accumulates()
    {
        var wallet = Wallet.CreateChecking(UserId, WalletName);

        wallet.Credit(50m);
        wallet.Credit(30m);

        wallet.Balance.ShouldBe(80m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Credit_NonPositive_Throws(decimal badAmount)
    {
        var wallet = Wallet.CreateChecking(UserId, WalletName);

        Should.Throw<ArgumentException>(() => wallet.Credit(badAmount));
    }

    [Fact]
    public void Debit_DecreasesBalance()
    {
        var wallet = Wallet.CreateChecking(UserId, WalletName);
        wallet.Credit(200m);

        wallet.Debit(50m);

        wallet.Balance.ShouldBe(150m);
    }

    [Fact]
    public void Debit_AllowNegativeBalance_CanGoBelowZero()
    {
        var wallet = Wallet.CreateChecking(UserId, WalletName, allowNegativeBalance: true);
        wallet.Credit(10m);

        wallet.Debit(50m);

        wallet.Balance.ShouldBe(-40m);
    }

    [Fact]
    public void Debit_DisallowNegativeBalance_ThrowsOnInsufficientFunds()
    {
        var wallet = Wallet.CreateChecking(UserId, WalletName, allowNegativeBalance: false);
        wallet.Credit(10m);

        Should.Throw<InvalidOperationException>(() => wallet.Debit(50m));
    }

    [Fact]
    public void UpdateName_SetsNewName()
    {
        var wallet = Wallet.CreateChecking(UserId, WalletName);

        wallet.UpdateName("Updated Name");

        wallet.Name.ShouldBe("Updated Name");
    }

    [Fact]
    public void Archive_SetsIsArchived()
    {
        var wallet = Wallet.CreateChecking(UserId, WalletName);

        wallet.Archive();

        wallet.IsArchived.ShouldBeTrue();
    }

    [Fact]
    public void Unarchive_ClearsIsArchived()
    {
        var wallet = Wallet.CreateChecking(UserId, WalletName);
        wallet.Archive();

        wallet.Unarchive();

        wallet.IsArchived.ShouldBeFalse();
    }

    [Fact]
    public void UpdateTarget_SetsTargetAmount()
    {
        var wallet = Wallet.CreateSavings(UserId, "Fund", 5000m);

        wallet.UpdateTarget(10_000m, null);

        wallet.TargetAmount.ShouldBe(10_000m);
    }

    [Fact]
    public void Credit_RoundsToTwoDecimals()
    {
        var wallet = Wallet.CreateChecking(UserId, WalletName);

        wallet.Credit(10.999m);

        wallet.Balance.ShouldBe(11.00m);
    }
}
