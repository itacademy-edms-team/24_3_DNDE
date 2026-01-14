using Ardalis.Result;
using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.Services;
using FinanceTrack.Finance.IntegrationTests.Data;

namespace FinanceTrack.Finance.IntegrationTests.Services;

public class UpdateExpenseFinancialTransactionServiceTests : BaseEfRepoTestFixture
{
    private const string UserId = "test-user";
    private readonly UpdateExpenseFinancialTransactionService _service;
    private readonly DateOnly _today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

    public UpdateExpenseFinancialTransactionServiceTests()
    {
        var repo = GetFinancialTransactionRepository();
        _service = new UpdateExpenseFinancialTransactionService(repo);
    }

    [Fact]
    public async Task UpdateExpenseFinancialTransaction_ExpenseDateIsBeforeIncomeDate_ReturnsError()
    {
        var repo = GetFinancialTransactionRepository();

        var income = FinancialTransaction.CreateIncome(
            userId: UserId,
            name: "Income",
            amount: 100m,
            operationDate: _today.AddDays(5),
            isMonthly: false
        );
        await repo.AddAsync(income);

        var expense = FinancialTransaction.CreateExpense(
            userId: UserId,
            name: "Expense",
            amount: 10m,
            operationDate: _today.AddDays(6),
            isMonthly: false,
            incomeTransactionId: income.Id
        );
        await repo.AddAsync(expense);

        var request = new UpdateExpenseFinancialTransactionRequest(
            TransactionId: expense.Id,
            UserId: UserId,
            Name: "Updated Expense",
            Amount: 15m,
            OperationDate: _today,
            IsMonthly: false,
            IncomeTransactionId: income.Id
        );

        var result = await _service.UpdateExpenseFinancialTransaction(
            request,
            CancellationToken.None
        );

        result.Status.ShouldBe(ResultStatus.Error);
        result.Errors.ShouldContain(
            "Expense operation date must be greater or equal than income operation date."
        );

        var unchangedExpense = await repo.GetByIdAsync(expense.Id);
        unchangedExpense.ShouldNotBeNull();
        unchangedExpense.OperationDate.ShouldBe(_today.AddDays(6));
        unchangedExpense.Name.ShouldBe("Expense");
    }

    [Fact]
    public async Task UpdateExpenseFinancialTransaction_DateIsEqualToIncomeDate_UpdatesExpense()
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

        var expense = FinancialTransaction.CreateExpense(
            userId: UserId,
            name: "Expense",
            amount: 10m,
            operationDate: _today.AddDays(3),
            isMonthly: false,
            incomeTransactionId: income.Id
        );
        await repo.AddAsync(expense);

        var request = new UpdateExpenseFinancialTransactionRequest(
            TransactionId: expense.Id,
            UserId: UserId,
            Name: "Updated Expense",
            Amount: 15m,
            OperationDate: _today,
            IsMonthly: false,
            IncomeTransactionId: income.Id
        );

        var result = await _service.UpdateExpenseFinancialTransaction(
            request,
            CancellationToken.None
        );

        result.Status.ShouldBe(ResultStatus.Ok);
        result.Value.OperationDate.ShouldBe(_today);
        result.Value.Name.ShouldBe("Updated Expense");
        result.Value.Amount.ShouldBe(15m);
        result.Value.IsMonthly.ShouldBeFalse();
    }

    [Fact]
    public async Task UpdateExpenseFinancialTransaction_DateIsAfterIncomeDate_UpdatesExpense()
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

        var expense = FinancialTransaction.CreateExpense(
            userId: UserId,
            name: "Expense",
            amount: 10m,
            operationDate: _today.AddDays(1),
            isMonthly: false,
            incomeTransactionId: income.Id
        );
        await repo.AddAsync(expense);

        var request = new UpdateExpenseFinancialTransactionRequest(
            TransactionId: expense.Id,
            UserId: UserId,
            Name: "Updated Expense",
            Amount: 20m,
            OperationDate: _today.AddDays(7),
            IsMonthly: false,
            IncomeTransactionId: income.Id
        );

        var result = await _service.UpdateExpenseFinancialTransaction(
            request,
            CancellationToken.None
        );

        result.Status.ShouldBe(ResultStatus.Ok);
        result.Value.OperationDate.ShouldBe(_today.AddDays(7));
        result.Value.Name.ShouldBe("Updated Expense");
        result.Value.Amount.ShouldBe(20m);
        result.Value.IsMonthly.ShouldBeFalse();
    }

    [Fact]
    public async Task UpdateExpenseFinancialTransaction_SettingExpenseToMonthlyForNonMonthlyIncome_ReturnsError()
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

        var expense = FinancialTransaction.CreateExpense(
            userId: UserId,
            name: "Expense",
            amount: 100m,
            operationDate: _today,
            isMonthly: false,
            incomeTransactionId: income.Id
        );
        await repo.AddAsync(expense);

        var request = new UpdateExpenseFinancialTransactionRequest(
            TransactionId: expense.Id,
            UserId: UserId,
            Name: "Monthly Expense",
            Amount: 150m,
            OperationDate: _today,
            IsMonthly: true,
            IncomeTransactionId: income.Id
        );

        var result = await _service.UpdateExpenseFinancialTransaction(
            request,
            CancellationToken.None
        );

        result.Status.ShouldBe(ResultStatus.Error);
        result.Errors.ShouldContain(
            "Cannot set expense to monthly when parent income is non-monthly. Expense must be non-monthly when income is non-monthly."
        );

        var unchangedExpense = await repo.GetByIdAsync(expense.Id);
        unchangedExpense.ShouldNotBeNull();
        unchangedExpense.IsMonthly.ShouldBeFalse();
        unchangedExpense.Name.ShouldBe("Expense");
    }

    [Fact]
    public async Task UpdateExpenseFinancialTransaction_ExpenseToNonMonthlyForNonMonthlyIncome_UpdatesExpense()
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

        var expense = FinancialTransaction.CreateExpense(
            userId: UserId,
            name: "Expense",
            amount: 100m,
            operationDate: _today,
            isMonthly: false,
            incomeTransactionId: income.Id
        );
        await repo.AddAsync(expense);

        var request = new UpdateExpenseFinancialTransactionRequest(
            TransactionId: expense.Id,
            UserId: UserId,
            Name: "Updated Expense",
            Amount: 150m,
            OperationDate: _today,
            IsMonthly: false,
            IncomeTransactionId: income.Id
        );

        var result = await _service.UpdateExpenseFinancialTransaction(
            request,
            CancellationToken.None
        );

        result.Status.ShouldBe(ResultStatus.Ok);
        result.Value.IsMonthly.ShouldBeFalse();
        result.Value.Name.ShouldBe("Updated Expense");
        result.Value.Amount.ShouldBe(150m);
    }

    [Fact]
    public async Task UpdateExpenseFinancialTransaction_ExpenseToMonthlyForMonthlyIncome_UpdatesExpense()
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

        var expense = FinancialTransaction.CreateExpense(
            userId: UserId,
            name: "Expense",
            amount: 500m,
            operationDate: _today,
            isMonthly: false,
            incomeTransactionId: income.Id
        );
        await repo.AddAsync(expense);

        var request = new UpdateExpenseFinancialTransactionRequest(
            TransactionId: expense.Id,
            UserId: UserId,
            Name: "Monthly Expense",
            Amount: 600m,
            OperationDate: _today,
            IsMonthly: true,
            IncomeTransactionId: income.Id
        );

        var result = await _service.UpdateExpenseFinancialTransaction(
            request,
            CancellationToken.None
        );

        result.Status.ShouldBe(ResultStatus.Ok);
        result.Value.IsMonthly.ShouldBeTrue();
        result.Value.Name.ShouldBe("Monthly Expense");
        result.Value.Amount.ShouldBe(600m);
    }

    [Fact]
    public async Task UpdateExpenseFinancialTransaction_ExpenseToNonMonthlyForMonthlyIncome_UpdatesExpense()
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

        var expense = FinancialTransaction.CreateExpense(
            userId: UserId,
            name: "Monthly Expense",
            amount: 500m,
            operationDate: _today,
            isMonthly: true,
            incomeTransactionId: income.Id
        );
        await repo.AddAsync(expense);

        var request = new UpdateExpenseFinancialTransactionRequest(
            TransactionId: expense.Id,
            UserId: UserId,
            Name: "One-time Expense",
            Amount: 600m,
            OperationDate: _today,
            IsMonthly: false,
            IncomeTransactionId: income.Id
        );

        var result = await _service.UpdateExpenseFinancialTransaction(
            request,
            CancellationToken.None
        );

        result.Status.ShouldBe(ResultStatus.Ok);
        result.Value.IsMonthly.ShouldBeFalse();
        result.Value.Name.ShouldBe("One-time Expense");
        result.Value.Amount.ShouldBe(600m);
    }
}
