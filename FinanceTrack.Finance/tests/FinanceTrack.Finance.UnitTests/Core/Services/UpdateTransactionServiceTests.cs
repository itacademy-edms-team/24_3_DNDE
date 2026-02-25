using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.Services;
using FinanceTrack.Finance.Core.WalletAggregate;

namespace FinanceTrack.Finance.UnitTests.Core.Services;

public class UpdateTransactionServiceTests
{
    private readonly IRepository<FinancialTransaction> _transactionRepo =
        Substitute.For<IRepository<FinancialTransaction>>();
    private readonly IRepository<Wallet> _walletRepo =
        Substitute.For<IRepository<Wallet>>();
    private readonly UpdateTransactionService _sut;

    private const string UserId = "user-1";
    private static readonly Guid WalletId = Guid.NewGuid();
    private static readonly DateOnly Today = new(2026, 2, 25);

    public UpdateTransactionServiceTests()
    {
        _sut = new UpdateTransactionService(_transactionRepo, _walletRepo);
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

        var request = new UpdateTransactionRequest(txId, UserId, "Updated", 100m, Today, null);
        var result = await _sut.Execute(request);

        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Execute_WrongUser_ReturnsForbidden()
    {
        var tx = FinancialTransaction.CreateIncome("other-user", WalletId, "Salary", 500m, Today);
        _transactionRepo.GetByIdAsync(tx.Id, Arg.Any<CancellationToken>())
            .Returns(tx);

        var request = new UpdateTransactionRequest(tx.Id, UserId, "Updated", 100m, Today, null);
        var result = await _sut.Execute(request);

        result.Status.ShouldBe(ResultStatus.Forbidden);
    }

    [Fact]
    public async Task Execute_TransferTransaction_ReturnsError()
    {
        var relatedId = Guid.NewGuid();
        var tx = FinancialTransaction.CreateTransferOut(UserId, WalletId, "Transfer", 200m, Today, relatedId);
        _transactionRepo.GetByIdAsync(tx.Id, Arg.Any<CancellationToken>())
            .Returns(tx);

        var request = new UpdateTransactionRequest(tx.Id, UserId, "Updated", 300m, Today, null);
        var result = await _sut.Execute(request);

        result.Status.ShouldBe(ResultStatus.Error);
    }

    [Fact]
    public async Task Execute_IncomeIncreased_CreditsWalletDifference()
    {
        // Income was 500, updating to 800 => wallet should be credited additional 300
        var tx = FinancialTransaction.CreateIncome(UserId, WalletId, "Salary", 500m, Today);
        var wallet = CreateFundedWallet(500m); // matches original income
        _transactionRepo.GetByIdAsync(tx.Id, Arg.Any<CancellationToken>())
            .Returns(tx);
        _walletRepo.GetByIdAsync(tx.WalletId, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var request = new UpdateTransactionRequest(tx.Id, UserId, "Updated Salary", 800m, Today, null);
        var result = await _sut.Execute(request);

        result.IsSuccess.ShouldBeTrue();
        wallet.Balance.ShouldBe(800m); // 500 + 300
        result.Value.Name.ShouldBe("Updated Salary");
        result.Value.Amount.ShouldBe(800m);
    }

    [Fact]
    public async Task Execute_IncomeDecreased_DebitsWalletDifference()
    {
        // Income was 500, updating to 200 => wallet should be debited 300
        var tx = FinancialTransaction.CreateIncome(UserId, WalletId, "Salary", 500m, Today);
        var wallet = CreateFundedWallet(500m);
        _transactionRepo.GetByIdAsync(tx.Id, Arg.Any<CancellationToken>())
            .Returns(tx);
        _walletRepo.GetByIdAsync(tx.WalletId, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var request = new UpdateTransactionRequest(tx.Id, UserId, "Reduced Salary", 200m, Today, null);
        var result = await _sut.Execute(request);

        result.IsSuccess.ShouldBeTrue();
        wallet.Balance.ShouldBe(200m); // 500 - 300
    }

    [Fact]
    public async Task Execute_ExpenseIncreased_DebitsWalletDifference()
    {
        // Expense was 100, updating to 250 => wallet should be debited additional 150
        var tx = FinancialTransaction.CreateExpense(UserId, WalletId, "Groceries", 100m, Today);
        var wallet = CreateFundedWallet(500m);
        _transactionRepo.GetByIdAsync(tx.Id, Arg.Any<CancellationToken>())
            .Returns(tx);
        _walletRepo.GetByIdAsync(tx.WalletId, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var request = new UpdateTransactionRequest(tx.Id, UserId, "More Groceries", 250m, Today, null);
        var result = await _sut.Execute(request);

        result.IsSuccess.ShouldBeTrue();
        wallet.Balance.ShouldBe(350m); // 500 - 150
    }

    [Fact]
    public async Task Execute_ExpenseDecreased_CreditsWalletDifference()
    {
        // Expense was 300, updating to 100 => wallet should be credited 200
        var tx = FinancialTransaction.CreateExpense(UserId, WalletId, "Groceries", 300m, Today);
        var wallet = CreateFundedWallet(500m);
        _transactionRepo.GetByIdAsync(tx.Id, Arg.Any<CancellationToken>())
            .Returns(tx);
        _walletRepo.GetByIdAsync(tx.WalletId, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var request = new UpdateTransactionRequest(tx.Id, UserId, "Less Groceries", 100m, Today, null);
        var result = await _sut.Execute(request);

        result.IsSuccess.ShouldBeTrue();
        wallet.Balance.ShouldBe(700m); // 500 + 200
    }

    [Fact]
    public async Task Execute_SameAmount_NoBalanceChange()
    {
        var tx = FinancialTransaction.CreateIncome(UserId, WalletId, "Salary", 500m, Today);
        var wallet = CreateFundedWallet(500m);
        _transactionRepo.GetByIdAsync(tx.Id, Arg.Any<CancellationToken>())
            .Returns(tx);
        _walletRepo.GetByIdAsync(tx.WalletId, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var request = new UpdateTransactionRequest(tx.Id, UserId, "Renamed Only", 500m, Today, null);
        var result = await _sut.Execute(request);

        result.IsSuccess.ShouldBeTrue();
        wallet.Balance.ShouldBe(500m); // unchanged
        result.Value.Name.ShouldBe("Renamed Only");
    }

    [Fact]
    public async Task Execute_UpdatesTransactionAndWalletInRepo()
    {
        var tx = FinancialTransaction.CreateIncome(UserId, WalletId, "Salary", 500m, Today);
        var wallet = CreateFundedWallet(500m);
        _transactionRepo.GetByIdAsync(tx.Id, Arg.Any<CancellationToken>())
            .Returns(tx);
        _walletRepo.GetByIdAsync(tx.WalletId, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var request = new UpdateTransactionRequest(tx.Id, UserId, "Updated", 600m, Today, null);
        await _sut.Execute(request);

        await _transactionRepo.Received(1)
            .UpdateAsync(tx, Arg.Any<CancellationToken>());
        await _walletRepo.Received(1)
            .UpdateAsync(wallet, Arg.Any<CancellationToken>());
    }
}
