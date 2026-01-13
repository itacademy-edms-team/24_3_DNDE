using Ardalis.Result;
using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.Services;
using FinanceTrack.Finance.IntegrationTests.Data;

namespace FinanceTrack.Finance.IntegrationTests.Services;

public class CreateExpenseFinancialTransactionService_DateValidation : BaseEfRepoTestFixture
{
    private readonly CreateExpenseFinancialTransactionService _service;
    private const string UserId = "test-user";
    private readonly DateOnly _today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

    public CreateExpenseFinancialTransactionService_DateValidation()
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
            OperationDate: _today, // та же дата
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
}
