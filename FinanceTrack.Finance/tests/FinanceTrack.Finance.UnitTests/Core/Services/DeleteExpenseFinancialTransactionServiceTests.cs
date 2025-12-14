using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.Services;

namespace FinanceTrack.Finance.UnitTests.Core.Services;

public class DeleteExpenseFinancialTransactionServiceTests
{
    private const string User = "user";
    private readonly IRepository<FinancialTransaction> _repo = Substitute.For<
        IRepository<FinancialTransaction>
    >();

    private readonly DeleteExpenseFinancialTransactionService _service;
    private readonly DateOnly _today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

    public DeleteExpenseFinancialTransactionServiceTests()
    {
        _service = new DeleteExpenseFinancialTransactionService(_repo);
    }

    [Fact]
    public async Task ReturnsNotFoundWhenMissing()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((FinancialTransaction?)null);

        var request = new DeleteExpenseFinancialTransactionRequest(
            TransactionId: Guid.NewGuid(),
            UserId: "user"
        );

        var result = await _service.DeleteExpenseFinancialTransaction(request, CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.NotFound);
        await _repo.DidNotReceiveWithAnyArgs().DeleteAsync(default!, default);
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

        var request = new DeleteExpenseFinancialTransactionRequest(
            TransactionId: expense.Id,
            UserId: User
        );

        var result = await _service.DeleteExpenseFinancialTransaction(request, CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.Forbidden);
        await _repo.DidNotReceiveWithAnyArgs().DeleteAsync(default!, default);
    }

    [Fact]
    public async Task ReturnsErrorWhenNotExpense()
    {
        var income = FinancialTransaction.CreateIncome(User, "Income", 100m, _today, false);
        _repo.GetByIdAsync(income.Id, Arg.Any<CancellationToken>()).Returns(income);

        var request = new DeleteExpenseFinancialTransactionRequest(
            TransactionId: income.Id,
            UserId: User
        );

        var result = await _service.DeleteExpenseFinancialTransaction(request, CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.Error);
        await _repo.DidNotReceiveWithAnyArgs().DeleteAsync(default!, default);
    }

    [Fact]
    public async Task DeletesExpense()
    {
        var expense = FinancialTransaction.CreateExpense(
            userId: User,
            name: "Expense",
            amount: 5m,
            operationDate: _today,
            isMonthly: false,
            incomeTransactionId: Guid.NewGuid()
        );
        _repo.GetByIdAsync(expense.Id, Arg.Any<CancellationToken>()).Returns(expense);

        var request = new DeleteExpenseFinancialTransactionRequest(
            TransactionId: expense.Id,
            UserId: User
        );

        var result = await _service.DeleteExpenseFinancialTransaction(request, CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.Ok);
        await _repo.Received(1).DeleteAsync(expense, Arg.Any<CancellationToken>());
    }
}

