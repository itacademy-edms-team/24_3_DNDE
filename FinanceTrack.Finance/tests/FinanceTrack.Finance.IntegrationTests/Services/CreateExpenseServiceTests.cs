using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.Services;
using FinanceTrack.Finance.Core.WalletAggregate;
using FinanceTrack.Finance.IntegrationTests.Data;

namespace FinanceTrack.Finance.IntegrationTests.Services;

public class CreateExpenseServiceTests : BaseEfRepoTestFixture
{
    private const string UserId = "user-1";
    private static readonly DateOnly Today = new(2026, 2, 25);

    private async Task<Wallet> CreateFundedWallet(decimal initialBalance, bool allowNeg = true)
    {
        var walletRepo = GetWalletRepository();
        var wallet = Wallet.CreateChecking(UserId, "Checking", allowNeg);

        // Apply initial balance before persisting so EF performs a single INSERT
        if (initialBalance > 0)
        {
            wallet.Credit(initialBalance);
        }

        await walletRepo.AddAsync(wallet);

        // Persist initial wallet state to the store
        await SaveChangesAsync();

        return wallet;
    }

    [Fact]
    public async Task Execute_ValidExpense_PersistsAndDebitsBalance()
    {
        var walletRepo = GetWalletRepository();
        var transactionRepo = GetFinancialTransactionRepository();

        var wallet = await CreateFundedWallet(1000m);

        var service = new CreateExpenseService(transactionRepo, walletRepo);
        var request = new CreateExpenseRequest(UserId, wallet.Id, "Groceries", 200m, Today);

        var result = await service.Execute(request);

        await SaveChangesAsync();

        result.IsSuccess.ShouldBeTrue();

        var updatedWallet = await walletRepo.GetByIdAsync(wallet.Id);
        updatedWallet.ShouldNotBeNull();
        updatedWallet.Balance.ShouldBe(800m);

        var transactions = await transactionRepo.ListAsync();
        transactions.Count.ShouldBe(1);
        transactions[0].TransactionType.ShouldBe(FinancialTransactionType.Expense);
    }

    [Fact]
    public async Task Execute_InsufficientFunds_NoNegative_ReturnsError()
    {
        var walletRepo = GetWalletRepository();
        var transactionRepo = GetFinancialTransactionRepository();

        var wallet = await CreateFundedWallet(50m, allowNeg: false);

        var service = new CreateExpenseService(transactionRepo, walletRepo);
        var request = new CreateExpenseRequest(UserId, wallet.Id, "Big Purchase", 100m, Today);

        var result = await service.Execute(request);

        await SaveChangesAsync();

        result.Status.ShouldBe(Ardalis.Result.ResultStatus.Error);

        // Balance should not change
        var updatedWallet = await walletRepo.GetByIdAsync(wallet.Id);
        updatedWallet!.Balance.ShouldBe(50m);

        // No transaction should be created
        (await transactionRepo.ListAsync()).Count.ShouldBe(0);
    }
}
