using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.Interfaces;
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

        // Arrange: create wallet and income using service
        var wallet = Wallet.CreateChecking(UserId, "Checking");
        await walletRepo.AddAsync(wallet);
        await SaveChangesAsync();

        var createIncomeService = new CreateIncomeService(transactionRepo, walletRepo);
        var createResult = await createIncomeService.Execute(
            new CreateIncomeRequest(UserId, wallet.Id, "Salary", 1000m, Today)
        );
        createResult.IsSuccess.ShouldBeTrue();

        await SaveChangesAsync();

        // Verify setup
        (await walletRepo.GetByIdAsync(wallet.Id))!.Balance.ShouldBe(1000m);

        // Act
        var service = new DeleteTransactionService(transactionRepo, walletRepo);
        var result = await service.Execute(createResult.Value, UserId);

        result.IsSuccess.ShouldBeTrue();
        await SaveChangesAsync();

        // Verify: Income reversed => balance should be 0
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

        // Arrange: wallet with balance 1000
        var wallet = Wallet.CreateChecking(UserId, "Checking");
        wallet.Credit(1000m);
        await walletRepo.AddAsync(wallet);
        await SaveChangesAsync();

        // Create expense using service
        var createExpenseService = new CreateExpenseService(transactionRepo, walletRepo);
        var createResult = await createExpenseService.Execute(
            new CreateExpenseRequest(UserId, wallet.Id, "Groceries", 200m, Today)
        );
        createResult.IsSuccess.ShouldBeTrue();

        await SaveChangesAsync();

        (await walletRepo.GetByIdAsync(wallet.Id))!.Balance.ShouldBe(800m);
        var createdTransactions = await transactionRepo.ListAsync();
        createdTransactions.Count.ShouldBe(1);
        createdTransactions[0].Id.ShouldBe(createResult.Value);

        // Act
        var service = new DeleteTransactionService(transactionRepo, walletRepo);
        var result = await service.Execute(createResult.Value, UserId);

        result.Status.ShouldBe(Ardalis.Result.ResultStatus.Ok);
        await SaveChangesAsync();

        // Verify: Expense reversed => balance should be 1000 again
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
