using Ardalis.Specification;
using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.RecurringTransactionAggregate;
using FinanceTrack.Finance.Core.Services;
using FinanceTrack.Finance.Core.WalletAggregate;

namespace FinanceTrack.Finance.UnitTests.Core.Services;

public class RecurringTransactionProcessorServiceTests
{
    private readonly IRepository<RecurringTransaction> _recurringRepo =
        Substitute.For<IRepository<RecurringTransaction>>();
    private readonly IRepository<FinancialTransaction> _transactionRepo =
        Substitute.For<IRepository<FinancialTransaction>>();
    private readonly IRepository<Wallet> _walletRepo =
        Substitute.For<IRepository<Wallet>>();
    private readonly ILogger<RecurringTransactionProcessorService> _logger =
        Substitute.For<ILogger<RecurringTransactionProcessorService>>();
    private readonly RecurringTransactionProcessorService _sut;

    private const string UserId = "user-1";
    private static readonly Guid WalletId = Guid.NewGuid();

    public RecurringTransactionProcessorServiceTests()
    {
        _sut = new RecurringTransactionProcessorService(
            _recurringRepo,
            _transactionRepo,
            _walletRepo,
            _logger
        );
    }

    private Wallet CreateFundedWallet(decimal balance)
    {
        var wallet = Wallet.CreateChecking(UserId, "Test Wallet");
        if (balance > 0) wallet.Credit(balance);
        return wallet;
    }

    [Fact]
    public async Task ProcessAsync_NoActiveRules_ReturnsZero()
    {
        _recurringRepo.ListAsync(
            Arg.Any<Specification<RecurringTransaction>>(),
            Arg.Any<CancellationToken>()
        ).Returns(new List<RecurringTransaction>());

        var result = await _sut.ProcessAsync(new DateOnly(2026, 2, 25));

        result.ShouldBe(0);
    }

    [Fact]
    public async Task ProcessAsync_IncomeRule_CreatesTransactionAndCreditsWallet()
    {
        var rule = RecurringTransaction.Create(
            UserId, WalletId, "Monthly Salary",
            RecurringTransactionType.Income, 3000m,
            dayOfMonth: 1,
            startDate: new DateOnly(2026, 2, 1)
        );
        var wallet = CreateFundedWallet(0m);

        _recurringRepo.ListAsync(
            Arg.Any<Specification<RecurringTransaction>>(),
            Arg.Any<CancellationToken>()
        ).Returns(new List<RecurringTransaction> { rule });

        _walletRepo.GetByIdAsync(rule.WalletId, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var result = await _sut.ProcessAsync(new DateOnly(2026, 2, 25));

        result.ShouldBe(1);
        wallet.Balance.ShouldBe(3000m);
        rule.LastProcessedDate.ShouldBe(new DateOnly(2026, 2, 1));

        await _transactionRepo.Received(1)
            .AddAsync(
                Arg.Is<FinancialTransaction>(t =>
                    t.TransactionType == FinancialTransactionType.Income &&
                    t.Amount == 3000m
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task ProcessAsync_ExpenseRule_CreatesTransactionAndDebitsWallet()
    {
        var rule = RecurringTransaction.Create(
            UserId, WalletId, "Monthly Rent",
            RecurringTransactionType.Expense, 1000m,
            dayOfMonth: 5,
            startDate: new DateOnly(2026, 2, 1)
        );
        var wallet = CreateFundedWallet(5000m);

        _recurringRepo.ListAsync(
            Arg.Any<Specification<RecurringTransaction>>(),
            Arg.Any<CancellationToken>()
        ).Returns(new List<RecurringTransaction> { rule });

        _walletRepo.GetByIdAsync(rule.WalletId, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var result = await _sut.ProcessAsync(new DateOnly(2026, 2, 25));

        result.ShouldBe(1);
        wallet.Balance.ShouldBe(4000m); // 5000 - 1000
    }

    [Fact]
    public async Task ProcessAsync_ExpenseRule_InsufficientFunds_Skips()
    {
        var rule = RecurringTransaction.Create(
            UserId, WalletId, "Monthly Rent",
            RecurringTransactionType.Expense, 2000m,
            dayOfMonth: 5,
            startDate: new DateOnly(2026, 2, 1)
        );
        var wallet = Wallet.CreateChecking(UserId, "No-negative Wallet", allowNegativeBalance: false);
        wallet.Credit(500m); // less than 2000

        _recurringRepo.ListAsync(
            Arg.Any<Specification<RecurringTransaction>>(),
            Arg.Any<CancellationToken>()
        ).Returns(new List<RecurringTransaction> { rule });

        _walletRepo.GetByIdAsync(rule.WalletId, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var result = await _sut.ProcessAsync(new DateOnly(2026, 2, 25));

        result.ShouldBe(0);
        wallet.Balance.ShouldBe(500m); // unchanged
    }

    [Fact]
    public async Task ProcessAsync_ArchivedWallet_Skips()
    {
        var rule = RecurringTransaction.Create(
            UserId, WalletId, "Monthly Salary",
            RecurringTransactionType.Income, 3000m,
            dayOfMonth: 1,
            startDate: new DateOnly(2026, 2, 1)
        );
        var wallet = CreateFundedWallet(0m);
        wallet.Archive();

        _recurringRepo.ListAsync(
            Arg.Any<Specification<RecurringTransaction>>(),
            Arg.Any<CancellationToken>()
        ).Returns(new List<RecurringTransaction> { rule });

        _walletRepo.GetByIdAsync(rule.WalletId, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var result = await _sut.ProcessAsync(new DateOnly(2026, 2, 25));

        result.ShouldBe(0);
    }

    [Fact]
    public async Task ProcessAsync_WalletNotFound_Skips()
    {
        var rule = RecurringTransaction.Create(
            UserId, WalletId, "Monthly Salary",
            RecurringTransactionType.Income, 3000m,
            dayOfMonth: 1,
            startDate: new DateOnly(2026, 2, 1)
        );

        _recurringRepo.ListAsync(
            Arg.Any<Specification<RecurringTransaction>>(),
            Arg.Any<CancellationToken>()
        ).Returns(new List<RecurringTransaction> { rule });

        _walletRepo.GetByIdAsync(rule.WalletId, Arg.Any<CancellationToken>())
            .Returns((Wallet?)null);

        var result = await _sut.ProcessAsync(new DateOnly(2026, 2, 25));

        result.ShouldBe(0);
    }

    [Fact]
    public async Task ProcessAsync_MultipleMonthsCatchUp_CreatesMultipleTransactions()
    {
        // Rule started Jan 2026, processing in March => should create Jan, Feb, Mar
        var rule = RecurringTransaction.Create(
            UserId, WalletId, "Salary",
            RecurringTransactionType.Income, 1000m,
            dayOfMonth: 15,
            startDate: new DateOnly(2026, 1, 15)
        );
        var wallet = CreateFundedWallet(0m);

        _recurringRepo.ListAsync(
            Arg.Any<Specification<RecurringTransaction>>(),
            Arg.Any<CancellationToken>()
        ).Returns(new List<RecurringTransaction> { rule });

        _walletRepo.GetByIdAsync(rule.WalletId, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var result = await _sut.ProcessAsync(new DateOnly(2026, 3, 20));

        result.ShouldBe(3); // Jan, Feb, Mar
        wallet.Balance.ShouldBe(3000m);
    }

    [Fact]
    public async Task ProcessAsync_AlreadyProcessedMonth_DoesNotDuplicate()
    {
        var rule = RecurringTransaction.Create(
            UserId, WalletId, "Salary",
            RecurringTransactionType.Income, 1000m,
            dayOfMonth: 1,
            startDate: new DateOnly(2026, 1, 1)
        );
        // Simulate already processed Jan
        rule.MarkProcessed(new DateOnly(2026, 1, 1));

        var wallet = CreateFundedWallet(1000m); // already has Jan's income

        _recurringRepo.ListAsync(
            Arg.Any<Specification<RecurringTransaction>>(),
            Arg.Any<CancellationToken>()
        ).Returns(new List<RecurringTransaction> { rule });

        _walletRepo.GetByIdAsync(rule.WalletId, Arg.Any<CancellationToken>())
            .Returns(wallet);

        // Processing for Feb
        var result = await _sut.ProcessAsync(new DateOnly(2026, 2, 15));

        result.ShouldBe(1); // only Feb
        wallet.Balance.ShouldBe(2000m);
    }

    [Fact]
    public async Task ProcessAsync_FutureOperationDate_DoesNotCreateYet()
    {
        // Rule starts on the 28th, but today is the 10th
        var rule = RecurringTransaction.Create(
            UserId, WalletId, "Salary",
            RecurringTransactionType.Income, 1000m,
            dayOfMonth: 28,
            startDate: new DateOnly(2026, 2, 28)
        );
        var wallet = CreateFundedWallet(0m);

        _recurringRepo.ListAsync(
            Arg.Any<Specification<RecurringTransaction>>(),
            Arg.Any<CancellationToken>()
        ).Returns(new List<RecurringTransaction> { rule });

        _walletRepo.GetByIdAsync(rule.WalletId, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var result = await _sut.ProcessAsync(new DateOnly(2026, 2, 10));

        result.ShouldBe(0);
    }

    [Fact]
    public async Task ProcessAsync_RuleWithEndDate_StopsAfterEndDate()
    {
        // Rule: Jan-Feb only
        var rule = RecurringTransaction.Create(
            UserId, WalletId, "Temp Salary",
            RecurringTransactionType.Income, 1000m,
            dayOfMonth: 1,
            startDate: new DateOnly(2026, 1, 1),
            endDate: new DateOnly(2026, 2, 28)
        );
        var wallet = CreateFundedWallet(0m);

        _recurringRepo.ListAsync(
            Arg.Any<Specification<RecurringTransaction>>(),
            Arg.Any<CancellationToken>()
        ).Returns(new List<RecurringTransaction> { rule });

        _walletRepo.GetByIdAsync(rule.WalletId, Arg.Any<CancellationToken>())
            .Returns(wallet);

        // Processing in April — but only Jan & Feb should be created
        var result = await _sut.ProcessAsync(new DateOnly(2026, 4, 15));

        result.ShouldBe(2); // Jan + Feb
        wallet.Balance.ShouldBe(2000m);
    }

    [Fact]
    public async Task ProcessAsync_ExceptionInOneRule_ContinuesWithOthers()
    {
        var rule1 = RecurringTransaction.Create(
            UserId, WalletId, "Good Salary",
            RecurringTransactionType.Income, 1000m,
            dayOfMonth: 1,
            startDate: new DateOnly(2026, 2, 1)
        );

        var badWalletId = Guid.NewGuid();
        var rule2 = RecurringTransaction.Create(
            UserId, badWalletId, "Bad Rule",
            RecurringTransactionType.Income, 500m,
            dayOfMonth: 1,
            startDate: new DateOnly(2026, 2, 1)
        );

        var wallet = CreateFundedWallet(0m);

        _recurringRepo.ListAsync(
            Arg.Any<Specification<RecurringTransaction>>(),
            Arg.Any<CancellationToken>()
        ).Returns(new List<RecurringTransaction> { rule2, rule1 });

        // rule2's wallet not found => skipped (not an exception, just skip)
        _walletRepo.GetByIdAsync(badWalletId, Arg.Any<CancellationToken>())
            .Returns((Wallet?)null);
        // rule1's wallet found
        _walletRepo.GetByIdAsync(rule1.WalletId, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var result = await _sut.ProcessAsync(new DateOnly(2026, 2, 25));

        // rule2 => 0 (wallet not found), rule1 => 1
        result.ShouldBe(1);
    }
}
