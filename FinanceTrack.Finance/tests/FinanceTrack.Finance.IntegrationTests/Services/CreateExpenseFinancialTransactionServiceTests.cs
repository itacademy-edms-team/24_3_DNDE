using Ardalis.Result;
using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.Services;
using FinanceTrack.Finance.IntegrationTests.Data;

namespace FinanceTrack.Finance.IntegrationTests.Services;

public class CreateExpenseFinancialTransactionServiceTests : BaseEfRepoTestFixture
{
    private const string UserId = "test-user";
    private readonly CreateExpenseFinancialTransactionService _service;
    private readonly DateOnly _today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

    public CreateExpenseFinancialTransactionServiceTests()
    {
        var repo = GetFinancialTransactionRepository();
        _service = new CreateExpenseFinancialTransactionService(repo);
    }

    [Fact]
    public async Task ReturnsErrorWhenExpenseDateIsBeforeIncomeDate()
    {
        var repo = GetFinancialTransactionRepository();

        var income = FinancialTransaction.CreateIncome(
            userId: UserId,
            name: "Income",
            amount: 100,
            operationDate: _today.AddDays(5),
            isMonthly: false
        );
        await repo.AddAsync(income);
        await repo.SaveChangesAsync();

        var request = new CreateExpenseFinancialTransactionRequest(
            UserId: UserId,
            Name: "Expense",
            Amount: 10,
            OperationDate: _today,
            IsMonthly: false,
            IncomeTransactionId: income.Id
        );

        var result = await _service.CreateExpenseFinancialTransaction(
            request,
            CancellationToken.None
        );

        result.Status.ShouldBe(ResultStatus.Error);
        result.Errors.ShouldContain(
            "Expense operation date must be greater or equal than income operation date."
        );

        var expenses = await repo.ListAsync();
        expenses.ShouldNotContain(e => e.Name == "Expense");
    }

    [Fact]
    public async Task CreatesExpenseWhenDateIsEqualToIncomeDate()
    {
        var repo = GetFinancialTransactionRepository();
        var income = FinancialTransaction.CreateIncome(
            userId: UserId,
            name: "Income",
            amount: 100m,
            operationDate: _today,
            isMonthly: false
        );
        await repo.AddAsync(income);
        await repo.SaveChangesAsync();

        var request = new CreateExpenseFinancialTransactionRequest(
            UserId: UserId,
            Name: "Expense",
            Amount: 10m,
            OperationDate: _today,
            IsMonthly: false,
            IncomeTransactionId: income.Id
        );

        var result = await _service.CreateExpenseFinancialTransaction(
            request,
            CancellationToken.None
        );

        result.Status.ShouldBe(ResultStatus.Ok);
        result.Value.ShouldNotBe(Guid.Empty);

        var createdExpense = await repo.GetByIdAsync(result.Value);
        createdExpense.ShouldNotBeNull();
        createdExpense.Name.ShouldBe("Expense");
        createdExpense.OperationDate.ShouldBe(_today);
    }

    [Fact]
    public async Task CreatesExpenseWhenDateIsAfterIncomeDate()
    {
        var repo = GetFinancialTransactionRepository();
        var income = FinancialTransaction.CreateIncome(
            userId: UserId,
            name: "Income",
            amount: 100m,
            operationDate: _today,
            isMonthly: false
        );
        await repo.AddAsync(income);
        await repo.SaveChangesAsync();

        var request = new CreateExpenseFinancialTransactionRequest(
            UserId: UserId,
            Name: "Expense",
            Amount: 10m,
            OperationDate: _today.AddDays(3), // расход позже
            IsMonthly: false,
            IncomeTransactionId: income.Id
        );

        var result = await _service.CreateExpenseFinancialTransaction(
            request,
            CancellationToken.None
        );

        result.Status.ShouldBe(ResultStatus.Ok);
        var createdExpense = await repo.GetByIdAsync(result.Value);
        createdExpense.ShouldNotBeNull();
        createdExpense.OperationDate.ShouldBe(_today.AddDays(3));
    }

    [Fact]
    public async Task ReturnsErrorWhenCreatingMonthlyExpenseForNonMonthlyIncome()
    {
        var repo = GetFinancialTransactionRepository();

        var income = FinancialTransaction.CreateIncome(
            userId: UserId,
            name: "One-time Income",
            amount: 1000m,
            operationDate: _today,
            isMonthly: false
        );
        await repo.AddAsync(income);
        await repo.SaveChangesAsync();

        var request = new CreateExpenseFinancialTransactionRequest(
            UserId: UserId,
            Name: "Monthly Expense",
            Amount: 100m,
            OperationDate: _today,
            IsMonthly: true,
            IncomeTransactionId: income.Id
        );

        var result = await _service.CreateExpenseFinancialTransaction(
            request,
            CancellationToken.None
        );

        result.Status.ShouldBe(ResultStatus.Error);
        result.Errors.ShouldContain(
            "Cannot create monthly expense for non-monthly income. Expense must be non-monthly when income is non-monthly."
        );

        var expenses = await repo.ListAsync();
        expenses.ShouldNotContain(e => e.Name == "Monthly Expense");
    }

    [Fact]
    public async Task CreatesNonMonthlyExpenseForNonMonthlyIncome()
    {
        var repo = GetFinancialTransactionRepository();

        var income = FinancialTransaction.CreateIncome(
            userId: UserId,
            name: "One-time Income",
            amount: 1000m,
            operationDate: _today,
            isMonthly: false
        );
        await repo.AddAsync(income);
        await repo.SaveChangesAsync();

        var request = new CreateExpenseFinancialTransactionRequest(
            UserId: UserId,
            Name: "One-time Expense",
            Amount: 100m,
            OperationDate: _today,
            IsMonthly: false,
            IncomeTransactionId: income.Id
        );

        var result = await _service.CreateExpenseFinancialTransaction(
            request,
            CancellationToken.None
        );

        result.Status.ShouldBe(ResultStatus.Ok);
        var createdExpense = await repo.GetByIdAsync(result.Value);
        createdExpense.ShouldNotBeNull();
        createdExpense.IsMonthly.ShouldBeFalse();
        createdExpense.Name.ShouldBe("One-time Expense");
        createdExpense.Amount.ShouldBe(100m);
    }

    [Fact]
    public async Task CreatesMonthlyExpenseForMonthlyIncome()
    {
        var repo = GetFinancialTransactionRepository();

        var income = FinancialTransaction.CreateIncome(
            userId: UserId,
            name: "Monthly Income",
            amount: 5000m,
            operationDate: _today,
            isMonthly: true
        );
        await repo.AddAsync(income);
        await repo.SaveChangesAsync();

        var request = new CreateExpenseFinancialTransactionRequest(
            UserId: UserId,
            Name: "Monthly Expense",
            Amount: 500m,
            OperationDate: _today,
            IsMonthly: true,
            IncomeTransactionId: income.Id
        );

        var result = await _service.CreateExpenseFinancialTransaction(
            request,
            CancellationToken.None
        );

        result.Status.ShouldBe(ResultStatus.Ok);
        var createdExpense = await repo.GetByIdAsync(result.Value);
        createdExpense.ShouldNotBeNull();
        createdExpense.IsMonthly.ShouldBeTrue();
        createdExpense.Name.ShouldBe("Monthly Expense");
    }

    [Fact]
    public async Task CreatesNonMonthlyExpenseForMonthlyIncome()
    {
        var repo = GetFinancialTransactionRepository();

        var income = FinancialTransaction.CreateIncome(
            userId: UserId,
            name: "Monthly Income",
            amount: 5000m,
            operationDate: _today,
            isMonthly: true
        );
        await repo.AddAsync(income);
        await repo.SaveChangesAsync();

        var request = new CreateExpenseFinancialTransactionRequest(
            UserId: UserId,
            Name: "One-time Expense",
            Amount: 500m,
            OperationDate: _today,
            IsMonthly: false,
            IncomeTransactionId: income.Id
        );

        var result = await _service.CreateExpenseFinancialTransaction(
            request,
            CancellationToken.None
        );

        result.Status.ShouldBe(ResultStatus.Ok);
        var createdExpense = await repo.GetByIdAsync(result.Value);
        createdExpense.ShouldNotBeNull();
        createdExpense.IsMonthly.ShouldBeFalse();
        createdExpense.Name.ShouldBe("One-time Expense");
    }
}
