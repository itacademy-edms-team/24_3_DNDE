using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.Services;
using FinanceTrack.Finance.Core.WalletAggregate;

namespace FinanceTrack.Finance.UnitTests.Core.Services;

public class CreateExpenseServiceTests
{
    private readonly IRepository<FinancialTransaction> _transactionRepo =
        Substitute.For<IRepository<FinancialTransaction>>();
    private readonly IRepository<Wallet> _walletRepo =
        Substitute.For<IRepository<Wallet>>();
    private readonly CreateExpenseService _sut;

    private const string UserId = "user-1";
    private static readonly Guid WalletId = Guid.NewGuid();
    private static readonly DateOnly Today = new(2026, 2, 25);

    public CreateExpenseServiceTests()
    {
        _sut = new CreateExpenseService(_transactionRepo, _walletRepo);
    }

    private Wallet CreateFundedWallet(decimal balance, bool allowNegative = true)
    {
        var wallet = Wallet.CreateChecking(UserId, "Test Wallet", allowNegative);
        if (balance > 0) wallet.Credit(balance);
        return wallet;
    }

    [Fact]
    public async Task Execute_WalletNotFound_ReturnsNotFound()
    {
        _walletRepo.GetByIdAsync(WalletId, Arg.Any<CancellationToken>())
            .Returns((Wallet?)null);

        var request = new CreateExpenseRequest(UserId, WalletId, "Groceries", 50m, Today);
        var result = await _sut.Execute(request);

        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Execute_WrongUser_ReturnsForbidden()
    {
        var wallet = Wallet.CreateChecking("other-user", "Other Wallet");
        _walletRepo.GetByIdAsync(WalletId, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var request = new CreateExpenseRequest(UserId, WalletId, "Groceries", 50m, Today);
        var result = await _sut.Execute(request);

        result.Status.ShouldBe(ResultStatus.Forbidden);
    }

    [Fact]
    public async Task Execute_ArchivedWallet_ReturnsError()
    {
        var wallet = CreateFundedWallet(1000m);
        wallet.Archive();
        _walletRepo.GetByIdAsync(WalletId, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var request = new CreateExpenseRequest(UserId, WalletId, "Groceries", 50m, Today);
        var result = await _sut.Execute(request);

        result.Status.ShouldBe(ResultStatus.Error);
    }

    [Fact]
    public async Task Execute_InsufficientFunds_NoNegativeAllowed_ReturnsError()
    {
        var wallet = CreateFundedWallet(30m, allowNegative: false);
        _walletRepo.GetByIdAsync(WalletId, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var request = new CreateExpenseRequest(UserId, WalletId, "Groceries", 50m, Today);
        var result = await _sut.Execute(request);

        result.Status.ShouldBe(ResultStatus.Error);
    }

    [Fact]
    public async Task Execute_ValidRequest_DebitsWalletAndAddsTransaction()
    {
        var wallet = CreateFundedWallet(200m);
        _walletRepo.GetByIdAsync(WalletId, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var request = new CreateExpenseRequest(UserId, WalletId, "Groceries", 50m, Today);
        var result = await _sut.Execute(request);

        result.IsSuccess.ShouldBeTrue();
        wallet.Balance.ShouldBe(150m);
        await _transactionRepo.Received(1)
            .AddAsync(
                Arg.Is<FinancialTransaction>(t =>
                    t.TransactionType == FinancialTransactionType.Expense
                ),
                Arg.Any<CancellationToken>()
            );
        await _walletRepo.Received(1)
            .UpdateAsync(wallet, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_AllowNegative_CanGoBelowZero()
    {
        var wallet = CreateFundedWallet(20m, allowNegative: true);
        _walletRepo.GetByIdAsync(WalletId, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var request = new CreateExpenseRequest(UserId, WalletId, "Big Purchase", 50m, Today);
        var result = await _sut.Execute(request);

        result.IsSuccess.ShouldBeTrue();
        wallet.Balance.ShouldBe(-30m);
    }
}
