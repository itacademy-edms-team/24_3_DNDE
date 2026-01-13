using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.Services;

namespace FinanceTrack.Finance.UnitTests.Core.Services;

public class UpdateExpenseFinancialTransactionServiceTests
{
    private const string User = "user";
    private readonly IRepository<FinancialTransaction> _repo = Substitute.For<
        IRepository<FinancialTransaction>
    >();

    private readonly UpdateExpenseFinancialTransactionService _service;
    private readonly DateOnly _today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

    public UpdateExpenseFinancialTransactionServiceTests()
    {
        _service = new UpdateExpenseFinancialTransactionService(_repo);
    }

    [Fact]
    public async Task ReturnsNotFoundWhenExpenseMissing()
    {
        _repo
            .GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((FinancialTransaction?)null);

        var request = new UpdateExpenseFinancialTransactionRequest(
            TransactionId: Guid.NewGuid(),
            UserId: "user",
            Name: "Expense",
            Amount: 10m,
            OperationDate: _today,
            IsMonthly: false,
            IncomeTransactionId: Guid.NewGuid()
        );

        var result = await _service.UpdateExpenseFinancialTransaction(
            request,
            CancellationToken.None
        );

        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task ReturnsForbiddenWhenUserMismatch()
    {
        var expense = FinancialTransaction.CreateExpense(
            userId: "other",
            name: "Expense",
            amount: 5m,
            operationDate: _today,
            isMonthly: false,
            incomeTransactionId: Guid.NewGuid()
        );
        _repo.GetByIdAsync(expense.Id, Arg.Any<CancellationToken>()).Returns(expense);

        var request = new UpdateExpenseFinancialTransactionRequest(
            TransactionId: expense.Id,
            UserId: User,
            Name: "Expense",
            Amount: 10m,
            OperationDate: _today,
            IsMonthly: false,
            IncomeTransactionId: expense.IncomeTransactionId!.Value
        );

        var result = await _service.UpdateExpenseFinancialTransaction(
            request,
            CancellationToken.None
        );

        result.Status.ShouldBe(ResultStatus.Forbidden);
        await _repo.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default);
    }

    [Fact]
    public async Task ReturnsErrorWhenTransactionNotExpense()
    {
        var income = FinancialTransaction.CreateIncome(User, "Income", 100m, _today, false);
        _repo.GetByIdAsync(income.Id, Arg.Any<CancellationToken>()).Returns(income);

        var request = new UpdateExpenseFinancialTransactionRequest(
            TransactionId: income.Id,
            UserId: User,
            Name: "Expense",
            Amount: 10m,
            OperationDate: _today,
            IsMonthly: false,
            IncomeTransactionId: Guid.NewGuid()
        );

        var result = await _service.UpdateExpenseFinancialTransaction(
            request,
            CancellationToken.None
        );

        result.Status.ShouldBe(ResultStatus.Error);
        await _repo.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default);
    }

    [Fact]
    public async Task ReturnsErrorWhenChangingParentIncome()
    {
        var originalIncomeId = Guid.NewGuid();
        var expense = FinancialTransaction.CreateExpense(
            userId: "user",
            name: "Expense",
            amount: 5m,
            operationDate: _today,
            isMonthly: false,
            incomeTransactionId: originalIncomeId
        );
        _repo.GetByIdAsync(expense.Id, Arg.Any<CancellationToken>()).Returns(expense);

        var request = new UpdateExpenseFinancialTransactionRequest(
            TransactionId: expense.Id,
            UserId: User,
            Name: "Expense",
            Amount: 10m,
            OperationDate: _today,
            IsMonthly: false,
            IncomeTransactionId: Guid.NewGuid()
        );

        var result = await _service.UpdateExpenseFinancialTransaction(
            request,
            CancellationToken.None
        );

        result.Status.ShouldBe(ResultStatus.Error);
        await _repo.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default);
    }

    [Fact]
    public async Task UpdatesExpenseFieldsAndCallsUpdate()
    {
        var incomeId = Guid.NewGuid();
        var expense = FinancialTransaction.CreateExpense(
            userId: "user",
            name: "Old",
            amount: 5m,
            operationDate: _today,
            isMonthly: false,
            incomeTransactionId: incomeId
        );
        _repo.GetByIdAsync(expense.Id, Arg.Any<CancellationToken>()).Returns(expense);

        var request = new UpdateExpenseFinancialTransactionRequest(
            TransactionId: expense.Id,
            UserId: User,
            Name: "New",
            Amount: 12.34m,
            OperationDate: _today.AddDays(1),
            IsMonthly: true,
            IncomeTransactionId: incomeId
        );

        var result = await _service.UpdateExpenseFinancialTransaction(
            request,
            CancellationToken.None
        );

        result.Status.ShouldBe(ResultStatus.Ok);
        expense.Name.ShouldBe("New");
        expense.Amount.ShouldBe(12.34m);
        expense.OperationDate.ShouldBe(_today.AddDays(1));
        expense.IsMonthly.ShouldBeTrue();
        await _repo.Received(1).UpdateAsync(expense, Arg.Any<CancellationToken>());
    }
}
