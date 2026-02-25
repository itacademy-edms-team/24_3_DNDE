using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.RecurringTransactionAggregate;
using FinanceTrack.Finance.Core.Services;
using FinanceTrack.Finance.Core.WalletAggregate;
using FinanceTrack.Finance.IntegrationTests.Data;
using Microsoft.Extensions.Logging;

namespace FinanceTrack.Finance.IntegrationTests.Services;

public class RecurringTransactionProcessorServiceTests : BaseEfRepoTestFixture
{
    private const string UserId = "user-1";

    [Fact]
    public async Task ProcessAsync_SingleIncomeRule_CreatesTransactionAndCreditsWallet()
    {
        var recurringRepo = GetRecurringTransactionRepository();
        var transactionRepo = GetFinancialTransactionRepository();
        var walletRepo = GetWalletRepository();
        var logger = Substitute.For<ILogger<RecurringTransactionProcessorService>>();

        // Setup wallet
        var wallet = Wallet.CreateChecking(UserId, "Salary Wallet");
        await walletRepo.AddAsync(wallet);

        // Setup recurring income rule
        var rule = RecurringTransaction.Create(
            UserId, wallet.Id, "Monthly Salary",
            RecurringTransactionType.Income, 5000m,
            dayOfMonth: 1,
            startDate: new DateOnly(2026, 2, 1)
        );
        await recurringRepo.AddAsync(rule);

        var service = new RecurringTransactionProcessorService(
            recurringRepo, transactionRepo, walletRepo, logger
        );

        var result = await service.ProcessAsync(new DateOnly(2026, 2, 25));

        result.ShouldBe(1);

        // Wallet should be credited
        var updatedWallet = await walletRepo.GetByIdAsync(wallet.Id);
        updatedWallet!.Balance.ShouldBe(5000m);

        // Transaction created
        var transactions = await transactionRepo.ListAsync();
        transactions.Count.ShouldBe(1);
        transactions[0].TransactionType.ShouldBe(FinancialTransactionType.Income);
        transactions[0].RecurringTransactionId.ShouldBe(rule.Id);

        // Rule updated
        var updatedRule = await recurringRepo.GetByIdAsync(rule.Id);
        updatedRule!.LastProcessedDate.ShouldBe(new DateOnly(2026, 2, 1));
    }

    [Fact]
    public async Task ProcessAsync_CatchUpMultipleMonths_CreatesAllDueTransactions()
    {
        var recurringRepo = GetRecurringTransactionRepository();
        var transactionRepo = GetFinancialTransactionRepository();
        var walletRepo = GetWalletRepository();
        var logger = Substitute.For<ILogger<RecurringTransactionProcessorService>>();

        var wallet = Wallet.CreateChecking(UserId, "Wallet");
        await walletRepo.AddAsync(wallet);

        // Rule started in Jan, processing in March
        var rule = RecurringTransaction.Create(
            UserId, wallet.Id, "Salary",
            RecurringTransactionType.Income, 1000m,
            dayOfMonth: 10,
            startDate: new DateOnly(2026, 1, 10)
        );
        await recurringRepo.AddAsync(rule);

        var service = new RecurringTransactionProcessorService(
            recurringRepo, transactionRepo, walletRepo, logger
        );

        var result = await service.ProcessAsync(new DateOnly(2026, 3, 15));

        result.ShouldBe(3); // Jan, Feb, Mar
        (await walletRepo.GetByIdAsync(wallet.Id))!.Balance.ShouldBe(3000m);
        (await transactionRepo.ListAsync()).Count.ShouldBe(3);
    }

    [Fact]
    public async Task ProcessAsync_ExpenseRule_DebitsWallet()
    {
        var recurringRepo = GetRecurringTransactionRepository();
        var transactionRepo = GetFinancialTransactionRepository();
        var walletRepo = GetWalletRepository();
        var logger = Substitute.For<ILogger<RecurringTransactionProcessorService>>();

        var wallet = Wallet.CreateChecking(UserId, "Wallet");
        wallet.Credit(10000m);
        await walletRepo.AddAsync(wallet);

        var rule = RecurringTransaction.Create(
            UserId, wallet.Id, "Rent",
            RecurringTransactionType.Expense, 2000m,
            dayOfMonth: 5,
            startDate: new DateOnly(2026, 2, 1)
        );
        await recurringRepo.AddAsync(rule);

        var service = new RecurringTransactionProcessorService(
            recurringRepo, transactionRepo, walletRepo, logger
        );

        var result = await service.ProcessAsync(new DateOnly(2026, 2, 25));

        result.ShouldBe(1);
        (await walletRepo.GetByIdAsync(wallet.Id))!.Balance.ShouldBe(8000m);

        var tx = (await transactionRepo.ListAsync()).Single();
        tx.TransactionType.ShouldBe(FinancialTransactionType.Expense);
        tx.Amount.ShouldBe(2000m);
    }

    [Fact]
    public async Task ProcessAsync_AlreadyProcessed_DoesNotDuplicate()
    {
        var recurringRepo = GetRecurringTransactionRepository();
        var transactionRepo = GetFinancialTransactionRepository();
        var walletRepo = GetWalletRepository();
        var logger = Substitute.For<ILogger<RecurringTransactionProcessorService>>();

        var wallet = Wallet.CreateChecking(UserId, "Wallet");
        wallet.Credit(5000m);
        await walletRepo.AddAsync(wallet);

        var rule = RecurringTransaction.Create(
            UserId, wallet.Id, "Salary",
            RecurringTransactionType.Income, 1000m,
            dayOfMonth: 1,
            startDate: new DateOnly(2026, 1, 1)
        );
        rule.MarkProcessed(new DateOnly(2026, 1, 1));
        await recurringRepo.AddAsync(rule);

        var service = new RecurringTransactionProcessorService(
            recurringRepo, transactionRepo, walletRepo, logger
        );

        // Processing for Feb only (Jan already processed)
        var result = await service.ProcessAsync(new DateOnly(2026, 2, 15));

        result.ShouldBe(1); // only Feb
        (await transactionRepo.ListAsync()).Count.ShouldBe(1);
    }
}
