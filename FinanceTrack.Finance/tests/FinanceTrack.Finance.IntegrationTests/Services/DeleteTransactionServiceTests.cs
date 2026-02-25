using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.Services;
using FinanceTrack.Finance.Core.WalletAggregate;
using FinanceTrack.Finance.IntegrationTests.Data;

namespace FinanceTrack.Finance.IntegrationTests.Services;

public class DeleteTransactionServiceTests : BaseEfRepoTestFixture
{
    private const string UserId = "user-1";
    private static readonly DateOnly Today = new(2026, 2, 25);

    [Fact]
    public async Task Execute_DeleteIncome_ReversesWalletBalance()
    {
        var walletRepo = GetWalletRepository();
        var transactionRepo = GetFinancialTransactionRepository();

        // Create wallet and income transaction
        var wallet = Wallet.CreateChecking(UserId, "Checking");
        await walletRepo.AddAsync(wallet);

        var tx = FinancialTransaction.CreateIncome(UserId, wallet.Id, "Salary", 1000m, Today);
        await transactionRepo.AddAsync(tx);
        wallet.Credit(1000m);
        await walletRepo.UpdateAsync(wallet);

        // Verify setup
        (await walletRepo.GetByIdAsync(wallet.Id))!.Balance.ShouldBe(1000m);

        var service = new DeleteTransactionService(transactionRepo, walletRepo);
        var result = await service.Execute(tx.Id, UserId);

        result.IsSuccess.ShouldBeTrue();

        // Income reversed => balance should be 0
        var updatedWallet = await walletRepo.GetByIdAsync(wallet.Id);
        updatedWallet!.Balance.ShouldBe(0m);

        // Transaction should be removed
        (await transactionRepo.ListAsync()).Count.ShouldBe(0);
    }

    [Fact]
    public async Task Execute_DeleteExpense_CreditsBackToWallet()
    {
        var walletRepo = GetWalletRepository();
        var transactionRepo = GetFinancialTransactionRepository();

        // Create wallet with balance and expense
        var wallet = Wallet.CreateChecking(UserId, "Checking");
        wallet.Credit(1000m);
        await walletRepo.AddAsync(wallet);

        var tx = FinancialTransaction.CreateExpense(UserId, wallet.Id, "Groceries", 200m, Today);
        await transactionRepo.AddAsync(tx);
        wallet.Debit(200m);
        await walletRepo.UpdateAsync(wallet);

        (await walletRepo.GetByIdAsync(wallet.Id))!.Balance.ShouldBe(800m);

        var service = new DeleteTransactionService(transactionRepo, walletRepo);
        var result = await service.Execute(tx.Id, UserId);

        result.IsSuccess.ShouldBeTrue();

        // Expense reversed => balance should be 1000 again
        var updatedWallet = await walletRepo.GetByIdAsync(wallet.Id);
        updatedWallet!.Balance.ShouldBe(1000m);

        (await transactionRepo.ListAsync()).Count.ShouldBe(0);
    }

    [Fact]
    public async Task Execute_TransactionNotFound_ReturnsNotFound()
    {
        var walletRepo = GetWalletRepository();
        var transactionRepo = GetFinancialTransactionRepository();

        var service = new DeleteTransactionService(transactionRepo, walletRepo);
        var result = await service.Execute(Guid.NewGuid(), UserId);

        result.Status.ShouldBe(Ardalis.Result.ResultStatus.NotFound);
    }
}
