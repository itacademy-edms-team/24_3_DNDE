using Ardalis.Result;
using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.Services;
using FinanceTrack.Finance.IntegrationTests.Data;

namespace FinanceTrack.Finance.IntegrationTests.Services;

public class UpdateIncomeFinancialTransactionServiceTests : BaseEfRepoTestFixture
{
    private const string UserId = "test-user";
    private readonly UpdateIncomeFinancialTransactionService _service;
    private readonly DateOnly _today = DateOnly.FromDateTime(DateTime.UtcNow);

    public UpdateIncomeFinancialTransactionServiceTests()
    {
        var repo = GetFinancialTransactionRepository();
        _service = new UpdateIncomeFinancialTransactionService(repo);
    }

    [Fact]
    public async Task ReturnsNotFoundWhenIncomeIsNotExist()
    {
        var missingIncome = new UpdateIncomeFinancialTransactionRequest(
            TransactionId: Guid.NewGuid(),
            UserId: UserId,
            Name: "Unexisting income",
            Amount: 10m,
            OperationDate: _today,
            IsMonthly: false
        );

        var result = await _service.UpdateIncome(missingIncome, CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task ReturnsForbiddenWhenUserMismatch()
    {
        var repo = GetFinancialTransactionRepository();

        var income = FinancialTransaction.CreateIncome(
            userId: "other-user",
            name: "Income",
            amount: 100m,
            operationDate: _today,
            isMonthly: false
        );
        await repo.AddAsync(income);

        var request = new UpdateIncomeFinancialTransactionRequest(
            TransactionId: income.Id,
            UserId: UserId,
            Name: "Updated Income",
            Amount: 150m,
            OperationDate: _today,
            IsMonthly: false
        );

        var result = await _service.UpdateIncome(request, CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.Forbidden);

        var unchangedIncome = await repo.GetByIdAsync(income.Id);
        unchangedIncome.ShouldNotBeNull();
        unchangedIncome.Name.ShouldBe("Income");
        unchangedIncome.Amount.ShouldBe(100m);
    }

    [Fact]
    public async Task ReturnsErrorWhenTransactionNotIncome()
    {
        var repo = GetFinancialTransactionRepository();

        var income = FinancialTransaction.CreateIncome(
            userId: UserId,
            name: "Parent Income",
            amount: 200m,
            operationDate: _today,
            isMonthly: false
        );
        await repo.AddAsync(income);

        var expense = FinancialTransaction.CreateExpense(
            userId: UserId,
            name: "Child expense",
            amount: 50m,
            operationDate: _today,
            isMonthly: false,
            incomeTransactionId: income.Id
        );
        await repo.AddAsync(expense);

        var request = new UpdateIncomeFinancialTransactionRequest(
            TransactionId: expense.Id,
            UserId: UserId,
            Name: "Updated",
            Amount: 60m,
            OperationDate: _today,
            IsMonthly: false
        );

        var result = await _service.UpdateIncome(request, CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.Error);
        result.Errors.ShouldContain("Only income transactions can be updated with this operation.");

        var unchangedExpense = await repo.GetByIdAsync(expense.Id);
        unchangedExpense.ShouldNotBeNull();
        unchangedExpense.Name.ShouldBe("Child expense");
        unchangedExpense.Amount.ShouldBe(50m);
    }

    [Fact]
    public async Task UpdatesIncomeFieldsSuccessfully()
    {
        var repo = GetFinancialTransactionRepository();

        var income = FinancialTransaction.CreateIncome(
            userId: UserId,
            name: "Old Income",
            amount: 100m,
            operationDate: _today,
            isMonthly: false
        );
        await repo.AddAsync(income);

        var request = new UpdateIncomeFinancialTransactionRequest(
            TransactionId: income.Id,
            UserId: UserId,
            Name: "New Income",
            Amount: 250.75m,
            OperationDate: _today.AddDays(5),
            IsMonthly: true
        );

        var result = await _service.UpdateIncome(request, CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.Ok);
        result.Value.ShouldNotBeNull();
        result.Value.Name.ShouldBe("New Income");
        result.Value.Amount.ShouldBe(250.75m);
        result.Value.OperationDate.ShouldBe(_today.AddDays(5));
        result.Value.IsMonthly.ShouldBeTrue();

        var updatedIncome = await repo.GetByIdAsync(income.Id);
        updatedIncome.ShouldNotBeNull();
        updatedIncome.Name.ShouldBe("New Income");
        updatedIncome.Amount.ShouldBe(250.75m);
        updatedIncome.OperationDate.ShouldBe(_today.AddDays(5));
        updatedIncome.IsMonthly.ShouldBeTrue();
    }

    [Fact]
    public async Task UpdatesOnlyNameAndAmount()
    {
        var repo = GetFinancialTransactionRepository();

        var originalDate = _today.AddDays(-10);
        var income = FinancialTransaction.CreateIncome(
            userId: UserId,
            name: "Original Name",
            amount: 100m,
            operationDate: originalDate,
            isMonthly: false
        );
        await repo.AddAsync(income);

        var request = new UpdateIncomeFinancialTransactionRequest(
            TransactionId: income.Id,
            UserId: UserId,
            Name: "Updated Name",
            Amount: 200m,
            OperationDate: originalDate,
            IsMonthly: false
        );

        var result = await _service.UpdateIncome(request, CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.Ok);
        result.Value.Name.ShouldBe("Updated Name");
        result.Value.Amount.ShouldBe(200m);
        result.Value.OperationDate.ShouldBe(originalDate);
        result.Value.IsMonthly.ShouldBeFalse();
    }

    [Fact]
    public async Task SetsExpensesToNonMonthlyWhenIncomeChangesFromMonthlyToNonMonthly()
    {
        var repo = GetFinancialTransactionRepository();

        var income = FinancialTransaction.CreateIncome(
            userId: UserId,
            name: "Monthly Income",
            amount: 1000m,
            operationDate: _today,
            isMonthly: true
        );
        await repo.AddAsync(income);

        var expense1 = FinancialTransaction.CreateExpense(
            userId: UserId,
            name: "Expense 1",
            amount: 100m,
            operationDate: _today.AddDays(1),
            isMonthly: true,
            incomeTransactionId: income.Id
        );
        var expense2 = FinancialTransaction.CreateExpense(
            userId: UserId,
            name: "Expense 2",
            amount: 200m,
            operationDate: _today.AddDays(2),
            isMonthly: true,
            incomeTransactionId: income.Id
        );
        await repo.AddAsync(expense1);
        await repo.AddAsync(expense2);

        var request = new UpdateIncomeFinancialTransactionRequest(
            TransactionId: income.Id,
            UserId: UserId,
            Name: "Monthly Income",
            Amount: 1000m,
            OperationDate: _today,
            IsMonthly: false
        );

        var result = await _service.UpdateIncome(request, CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.Ok);
        result.Value.IsMonthly.ShouldBeFalse();

        var updatedIncome = await repo.GetByIdAsync(income.Id);
        updatedIncome.ShouldNotBeNull();
        updatedIncome.IsMonthly.ShouldBeFalse();

        var updatedExpense1 = await repo.GetByIdAsync(expense1.Id);
        updatedExpense1.ShouldNotBeNull();
        updatedExpense1.IsMonthly.ShouldBeFalse();

        var updatedExpense2 = await repo.GetByIdAsync(expense2.Id);
        updatedExpense2.ShouldNotBeNull();
        updatedExpense2.IsMonthly.ShouldBeFalse();
    }

    [Fact]
    public async Task UpdatesIncomeWhenNoExpensesExist()
    {
        var repo = GetFinancialTransactionRepository();

        var income = FinancialTransaction.CreateIncome(
            userId: UserId,
            name: "Monthly Income",
            amount: 1000m,
            operationDate: _today,
            isMonthly: true
        );
        await repo.AddAsync(income);

        var request = new UpdateIncomeFinancialTransactionRequest(
            TransactionId: income.Id,
            UserId: UserId,
            Name: "Updated Income",
            Amount: 1000m,
            OperationDate: _today,
            IsMonthly: false
        );

        var result = await _service.UpdateIncome(request, CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.Ok);
        result.Value.IsMonthly.ShouldBeFalse();

        var updatedIncome = await repo.GetByIdAsync(income.Id);
        updatedIncome.ShouldNotBeNull();
        updatedIncome.IsMonthly.ShouldBeFalse();
    }

    [Fact]
    public async Task DoesNotChangeExpensesWhenIncomeWasAlreadyNonMonthly()
    {
        var repo = GetFinancialTransactionRepository();

        var income = FinancialTransaction.CreateIncome(
            userId: UserId,
            name: "Non-Monthly Income",
            amount: 1000m,
            operationDate: _today,
            isMonthly: false
        );
        await repo.AddAsync(income);

        var expense = FinancialTransaction.CreateExpense(
            userId: UserId,
            name: "Monthly Expense",
            amount: 100m,
            operationDate: _today.AddDays(1),
            isMonthly: true,
            incomeTransactionId: income.Id
        );
        await repo.AddAsync(expense);

        var request = new UpdateIncomeFinancialTransactionRequest(
            TransactionId: income.Id,
            UserId: UserId,
            Name: "Updated Income",
            Amount: 1500m,
            OperationDate: _today,
            IsMonthly: false
        );

        var result = await _service.UpdateIncome(request, CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.Ok);

        var unchangedExpense = await repo.GetByIdAsync(expense.Id);
        unchangedExpense.ShouldNotBeNull();
        unchangedExpense.IsMonthly.ShouldBeTrue();
    }
}
