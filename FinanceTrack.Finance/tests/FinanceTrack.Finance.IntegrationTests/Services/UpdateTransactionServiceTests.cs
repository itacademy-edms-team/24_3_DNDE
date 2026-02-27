using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.Services;
using FinanceTrack.Finance.Core.WalletAggregate;
using FinanceTrack.Finance.IntegrationTests.Data;

namespace FinanceTrack.Finance.IntegrationTests.Services;

public class UpdateTransactionServiceTests : BaseEfRepoTestFixture
{
    private const string UserId = "user-1";
    private static readonly DateOnly Today = new(2026, 2, 25);

    [Fact]
    public async Task Execute_IncreaseIncomeAmount_AdjustsWalletBalance()
    {
        var walletRepo = GetWalletRepository();
        var transactionRepo = GetFinancialTransactionRepository();

        // Setup: wallet with income (wallet balance already includes original income)
        var wallet = Wallet.CreateChecking(UserId, "Checking");
        wallet.Credit(500m); // starting balance reflecting original income
        await walletRepo.AddAsync(wallet);

        var tx = FinancialTransaction.CreateIncome(UserId, wallet.Id, "Salary", 500m, Today);
        await transactionRepo.AddAsync(tx);

        await SaveChangesAsync();

        // Act: increase income to 800
        var service = new UpdateTransactionService(transactionRepo, walletRepo);
        var request = new UpdateTransactionRequest(tx.Id, UserId, "Updated Salary", 800m, Today, null);
        var result = await service.Execute(request);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Amount.ShouldBe(800m);
        result.Value.Name.ShouldBe("Updated Salary");
        var updatedWallet = await walletRepo.GetByIdAsync(wallet.Id);
        updatedWallet!.Balance.ShouldBe(800m); // 500 + 300
    }

    [Fact]
    public async Task Execute_DecreaseExpenseAmount_AdjustsWalletBalance()
    {
        var walletRepo = GetWalletRepository();
        var transactionRepo = GetFinancialTransactionRepository();

        // Setup: wallet=1000, expense=300 => wallet=700
        var wallet = Wallet.CreateChecking(UserId, "Checking");
        wallet.Credit(1000m);
        wallet.Debit(300m); // apply initial expense effect to wallet balance
        await walletRepo.AddAsync(wallet);

        var tx = FinancialTransaction.CreateExpense(UserId, wallet.Id, "Groceries", 300m, Today);
        await transactionRepo.AddAsync(tx);

        await SaveChangesAsync();

        // Act: decrease expense to 100
        var service = new UpdateTransactionService(transactionRepo, walletRepo);
        var request = new UpdateTransactionRequest(tx.Id, UserId, "Less Groceries", 100m, Today, null);
        var result = await service.Execute(request);

        // Assert: wallet should gain back 200 (from 700 to 900)
        result.IsSuccess.ShouldBeTrue();
        var updatedWallet = await walletRepo.GetByIdAsync(wallet.Id);
        updatedWallet!.Balance.ShouldBe(900m);
    }

    [Fact]
    public async Task Execute_TransferTransaction_ReturnsError()
    {
        var walletRepo = GetWalletRepository();
        var transactionRepo = GetFinancialTransactionRepository();

        var wallet = Wallet.CreateChecking(UserId, "Checking");
        await walletRepo.AddAsync(wallet);

        var tx = FinancialTransaction.CreateTransferOut(
            UserId, wallet.Id, "Transfer", 100m, Today, Guid.NewGuid()
        );
        await transactionRepo.AddAsync(tx);

        await SaveChangesAsync();

        var service = new UpdateTransactionService(transactionRepo, walletRepo);
        var request = new UpdateTransactionRequest(tx.Id, UserId, "Updated", 200m, Today, null);
        var result = await service.Execute(request);

        await SaveChangesAsync();

        result.Status.ShouldBe(Ardalis.Result.ResultStatus.Error);
    }
}
