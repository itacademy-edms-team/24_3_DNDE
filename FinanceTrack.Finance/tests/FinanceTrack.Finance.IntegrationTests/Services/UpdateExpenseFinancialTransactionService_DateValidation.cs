using Ardalis.Result;
using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.Services;

namespace FinanceTrack.Finance.IntegrationTests.Data;

public class UpdateExpenseFinancialTransactionService_DateValidation : BaseEfRepoTestFixture
{
    private readonly UpdateExpenseFinancialTransactionService _service;
    private const string UserId = "test-user";
    private readonly DateOnly _today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

    public UpdateExpenseFinancialTransactionService_DateValidation()
    {
        var repo = GetFinancialTransactionRepository();
        _service = new UpdateExpenseFinancialTransactionService(repo);
    }

    [Fact]
    public async Task ReturnsErrorWhenExpenseDateIsBeforeIncomeDate()
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
        await repo.SaveChangesAsync();

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
    public async Task UpdatesExpenseWhenDateIsEqualToIncomeDate()
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
        await repo.SaveChangesAsync();

        var request = new UpdateExpenseFinancialTransactionRequest(
            TransactionId: expense.Id,
            UserId: UserId,
            Name: "Updated Expense",
            Amount: 15m,
            OperationDate: _today, // та же дата что и у дохода
            IsMonthly: true,
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
        result.Value.IsMonthly.ShouldBeTrue();
    }

    [Fact]
    public async Task UpdatesExpenseWhenDateIsAfterIncomeDate()
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
        await repo.SaveChangesAsync();

        var request = new UpdateExpenseFinancialTransactionRequest(
            TransactionId: expense.Id,
            UserId: UserId,
            Name: "Updated Expense",
            Amount: 20m,
            OperationDate: _today.AddDays(7),
            IsMonthly: true,
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
    }
}
