using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.Services;
using FinanceTrack.Finance.Core.WalletAggregate;
using FinanceTrack.Finance.IntegrationTests.Data;

namespace FinanceTrack.Finance.IntegrationTests.Services;

public class CreateIncomeServiceTests : BaseEfRepoTestFixture
{
    private const string UserId = "user-1";
    private static readonly DateOnly Today = new(2026, 2, 25);

    [Fact]
    public async Task Execute_ValidRequest_PersistsTransactionAndUpdatesBalance()
    {
        var walletRepo = GetWalletRepository();
        var transactionRepo = GetFinancialTransactionRepository();
        // Arrange: create and persist a wallet
        var wallet = Wallet.CreateChecking(UserId, "Checking");
        await walletRepo.AddAsync(wallet);
        var walletId = wallet.Id;

        // Persist initial wallet state to the store so services can load it via specs
        await SaveChangesAsync();

        var service = new CreateIncomeService(transactionRepo, walletRepo);
        var request = new CreateIncomeRequest(UserId, walletId, "Salary", 5000m, Today);

        // Act
        var result = await service.Execute(request);

        // Persist effects of the service (mimic UnitOfWork behavior)
        await SaveChangesAsync();

        // Assert (temporary diagnostic)
        if (!result.IsSuccess)
        {
            throw new Exception(
                $"CreateIncomeService failed. Status={result.Status}, Errors={string.Join(" | ", result.Errors)}"
            );
        }

        // Verify wallet balance
        var updatedWallet = await walletRepo.GetByIdAsync(walletId);
        updatedWallet.ShouldNotBeNull();
        updatedWallet.Balance.ShouldBe(5000m);

        // Verify transaction persisted
        var transactions = await transactionRepo.ListAsync();
        transactions.Count.ShouldBe(1);
        transactions[0].TransactionType.ShouldBe(FinancialTransactionType.Income);
        transactions[0].Amount.ShouldBe(5000m);
        transactions[0].WalletId.ShouldBe(walletId);
    }

    [Fact]
    public async Task Execute_MultipleIncomes_AccumulatesBalance()
    {
        var walletRepo = GetWalletRepository();
        var transactionRepo = GetFinancialTransactionRepository();

        var wallet = Wallet.CreateChecking(UserId, "Checking");
        await walletRepo.AddAsync(wallet);
        await SaveChangesAsync();

        var service = new CreateIncomeService(transactionRepo, walletRepo);

        await service.Execute(new CreateIncomeRequest(UserId, wallet.Id, "Salary", 3000m, Today));
        await SaveChangesAsync();

        await service.Execute(new CreateIncomeRequest(UserId, wallet.Id, "Bonus", 500m, Today));
        await SaveChangesAsync();

        var updatedWallet = await walletRepo.GetByIdAsync(wallet.Id);
        updatedWallet.ShouldNotBeNull();
        updatedWallet.Balance.ShouldBe(3500m);

        var transactions = await transactionRepo.ListAsync();
        transactions.Count.ShouldBe(2);
    }
}
