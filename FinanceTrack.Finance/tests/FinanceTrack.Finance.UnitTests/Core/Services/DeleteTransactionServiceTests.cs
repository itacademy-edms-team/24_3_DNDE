using Ardalis.Specification;
using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.Services;
using FinanceTrack.Finance.Core.WalletAggregate;

namespace FinanceTrack.Finance.UnitTests.Core.Services;

public class DeleteTransactionServiceTests
{
    private readonly IRepository<FinancialTransaction> _transactionRepo =
        Substitute.For<IRepository<FinancialTransaction>>();
    private readonly IRepository<Wallet> _walletRepo =
        Substitute.For<IRepository<Wallet>>();
    private readonly DeleteTransactionService _sut;

    private const string UserId = "user-1";
    private static readonly Guid WalletId = Guid.NewGuid();
    private static readonly DateOnly Today = new(2026, 2, 25);

    public DeleteTransactionServiceTests()
    {
        _sut = new DeleteTransactionService(_transactionRepo, _walletRepo);
    }

    private Wallet CreateFundedWallet(decimal balance)
    {
        var wallet = Wallet.CreateChecking(UserId, "Test Wallet");
        if (balance > 0) wallet.Credit(balance);
        return wallet;
    }

    [Fact]
    public async Task Execute_TransactionNotFound_ReturnsNotFound()
    {
        var txId = Guid.NewGuid();
        _transactionRepo.GetByIdAsync(txId, Arg.Any<CancellationToken>())
            .Returns((FinancialTransaction?)null);

        var result = await _sut.Execute(txId, UserId);

        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Execute_WrongUser_ReturnsForbidden()
    {
        var tx = FinancialTransaction.CreateIncome("other-user", WalletId, "Salary", 100m, Today);
        _transactionRepo.GetByIdAsync(tx.Id, Arg.Any<CancellationToken>())
            .Returns(tx);

        var result = await _sut.Execute(tx.Id, UserId);

        result.Status.ShouldBe(ResultStatus.Forbidden);
    }

    [Fact]
    public async Task Execute_DeleteIncome_DebitsWallet()
    {
        var tx = FinancialTransaction.CreateIncome(UserId, WalletId, "Salary", 500m, Today);
        var wallet = CreateFundedWallet(500m);

        _transactionRepo.GetByIdAsync(tx.Id, Arg.Any<CancellationToken>())
            .Returns(tx);
        _walletRepo.GetByIdAsync(tx.WalletId, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var result = await _sut.Execute(tx.Id, UserId);

        result.IsSuccess.ShouldBeTrue();
        wallet.Balance.ShouldBe(0m); // 500 - 500 = 0
        await _transactionRepo.Received(1)
            .DeleteAsync(tx, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_DeleteExpense_CreditsWallet()
    {
        var tx = FinancialTransaction.CreateExpense(UserId, WalletId, "Groceries", 100m, Today);
        var wallet = CreateFundedWallet(200m);

        _transactionRepo.GetByIdAsync(tx.Id, Arg.Any<CancellationToken>())
            .Returns(tx);
        _walletRepo.GetByIdAsync(tx.WalletId, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var result = await _sut.Execute(tx.Id, UserId);

        result.IsSuccess.ShouldBeTrue();
        wallet.Balance.ShouldBe(300m); // 200 + 100 = 300
    }

    [Fact]
    public async Task Execute_WalletNotFound_ReturnsNotFound()
    {
        var tx = FinancialTransaction.CreateIncome(UserId, WalletId, "Salary", 500m, Today);
        _transactionRepo.GetByIdAsync(tx.Id, Arg.Any<CancellationToken>())
            .Returns(tx);
        _walletRepo.GetByIdAsync(tx.WalletId, Arg.Any<CancellationToken>())
            .Returns((Wallet?)null);

        var result = await _sut.Execute(tx.Id, UserId);

        result.Status.ShouldBe(ResultStatus.NotFound);
    }
}
