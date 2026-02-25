using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.Services;
using FinanceTrack.Finance.Core.WalletAggregate;

namespace FinanceTrack.Finance.UnitTests.Core.Services;

public class CreateIncomeServiceTests
{
    private readonly IRepository<FinancialTransaction> _transactionRepo =
        Substitute.For<IRepository<FinancialTransaction>>();
    private readonly IRepository<Wallet> _walletRepo =
        Substitute.For<IRepository<Wallet>>();
    private readonly CreateIncomeService _sut;

    private const string UserId = "user-1";
    private static readonly Guid WalletId = Guid.NewGuid();
    private static readonly DateOnly Today = new(2026, 2, 25);

    public CreateIncomeServiceTests()
    {
        _sut = new CreateIncomeService(_transactionRepo, _walletRepo);
    }

    private Wallet CreateTestWallet(bool archived = false)
    {
        var wallet = Wallet.CreateChecking(UserId, "Test Wallet");
        if (archived) wallet.Archive();
        return wallet;
    }

    [Fact]
    public async Task Execute_WalletNotFound_ReturnsNotFound()
    {
        _walletRepo.GetByIdAsync(WalletId, Arg.Any<CancellationToken>())
            .Returns((Wallet?)null);

        var request = new CreateIncomeRequest(UserId, WalletId, "Salary", 1000m, Today);
        var result = await _sut.Execute(request);

        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Execute_WrongUser_ReturnsForbidden()
    {
        var wallet = Wallet.CreateChecking("other-user", "Other Wallet");
        _walletRepo.GetByIdAsync(WalletId, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var request = new CreateIncomeRequest(UserId, WalletId, "Salary", 1000m, Today);
        var result = await _sut.Execute(request);

        result.Status.ShouldBe(ResultStatus.Forbidden);
    }

    [Fact]
    public async Task Execute_ArchivedWallet_ReturnsError()
    {
        var wallet = CreateTestWallet(archived: true);
        _walletRepo.GetByIdAsync(WalletId, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var request = new CreateIncomeRequest(UserId, WalletId, "Salary", 1000m, Today);
        var result = await _sut.Execute(request);

        result.Status.ShouldBe(ResultStatus.Error);
    }

    [Fact]
    public async Task Execute_ValidRequest_CreditsWallet()
    {
        var wallet = CreateTestWallet();
        _walletRepo.GetByIdAsync(WalletId, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var request = new CreateIncomeRequest(UserId, WalletId, "Salary", 500m, Today);
        var result = await _sut.Execute(request);

        result.IsSuccess.ShouldBeTrue();
        wallet.Balance.ShouldBe(500m);
    }

    [Fact]
    public async Task Execute_ValidRequest_AddsTransactionAndUpdatesWallet()
    {
        var wallet = CreateTestWallet();
        _walletRepo.GetByIdAsync(WalletId, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var request = new CreateIncomeRequest(UserId, WalletId, "Salary", 1000m, Today);
        await _sut.Execute(request);

        await _transactionRepo.Received(1)
            .AddAsync(Arg.Any<FinancialTransaction>(), Arg.Any<CancellationToken>());
        await _walletRepo.Received(1)
            .UpdateAsync(wallet, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_WithCategory_PassesCategoryToTransaction()
    {
        var wallet = CreateTestWallet();
        _walletRepo.GetByIdAsync(WalletId, Arg.Any<CancellationToken>())
            .Returns(wallet);
        var categoryId = Guid.NewGuid();

        var request = new CreateIncomeRequest(UserId, WalletId, "Salary", 500m, Today, categoryId);
        var result = await _sut.Execute(request);

        result.IsSuccess.ShouldBeTrue();
        await _transactionRepo.Received(1)
            .AddAsync(
                Arg.Is<FinancialTransaction>(t => t.CategoryId == categoryId),
                Arg.Any<CancellationToken>()
            );
    }
}
