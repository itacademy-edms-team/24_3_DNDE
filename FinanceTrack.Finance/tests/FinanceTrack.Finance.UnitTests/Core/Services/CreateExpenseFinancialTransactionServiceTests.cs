using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.Services;

namespace FinanceTrack.Finance.UnitTests.Core.Services;

public class CreateExpenseFinancialTransactionServiceTests
{
    private const string User = "user";
    private readonly DateOnly _today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

    private readonly IRepository<FinancialTransaction> _repo = Substitute.For<
        IRepository<FinancialTransaction>
    >();

    private readonly CreateExpenseFinancialTransactionService _service;

    public CreateExpenseFinancialTransactionServiceTests()
    {
        _service = new CreateExpenseFinancialTransactionService(_repo);
    }

    [Fact]
    public async Task ReturnsNotFoundWhenIncomeMissing()
    {
        var incomeId = Guid.NewGuid();
        _repo
            .GetByIdAsync(incomeId, Arg.Any<CancellationToken>())
            .Returns((FinancialTransaction?)null);

        var request = new CreateExpenseFinancialTransactionRequest(
            UserId: User,
            Name: "Expense",
            Amount: 10m,
            OperationDate: _today,
            IsMonthly: false,
            IncomeTransactionId: incomeId
        );

        var result = await _service.CreateExpenseFinancialTransaction(
            request,
            CancellationToken.None
        );

        result.Status.ShouldBe(ResultStatus.NotFound);
        await _repo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task ReturnsForbiddenWhenIncomeOwnedByOtherUser()
    {
        var incomeId = Guid.NewGuid();
        var income = FinancialTransaction.CreateIncome(
            userId: "other",
            name: "Income",
            amount: 100m,
            operationDate: _today,
            isMonthly: false
        );
        _repo.GetByIdAsync(incomeId, Arg.Any<CancellationToken>()).Returns(income);

        var request = new CreateExpenseFinancialTransactionRequest(
            UserId: User,
            Name: "Expense",
            Amount: 10m,
            OperationDate: _today,
            IsMonthly: false,
            IncomeTransactionId: incomeId
        );

        var result = await _service.CreateExpenseFinancialTransaction(
            request,
            CancellationToken.None
        );

        result.Status.ShouldBe(ResultStatus.Forbidden);
        await _repo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task ReturnsErrorWhenParentNotIncome()
    {
        var incomeId = Guid.NewGuid();
        var notIncome = FinancialTransaction.CreateExpense(
            userId: User,
            name: "ExistingExpense",
            amount: 5m,
            operationDate: _today,
            isMonthly: false,
            incomeTransactionId: incomeId
        );
        _repo.GetByIdAsync(incomeId, Arg.Any<CancellationToken>()).Returns(notIncome);

        var request = new CreateExpenseFinancialTransactionRequest(
            UserId: User,
            Name: "Expense",
            Amount: 10m,
            OperationDate: _today,
            IsMonthly: false,
            IncomeTransactionId: incomeId
        );

        var result = await _service.CreateExpenseFinancialTransaction(
            request,
            CancellationToken.None
        );

        result.Status.ShouldBe(ResultStatus.Error);
        await _repo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task CreatesExpenseAndReturnsId()
    {
        var incomeId = Guid.NewGuid();
        var income = FinancialTransaction.CreateIncome(
            userId: User,
            name: "Income",
            amount: 100m,
            operationDate: _today,
            isMonthly: true
        );
        _repo.GetByIdAsync(incomeId, Arg.Any<CancellationToken>()).Returns(income);

        FinancialTransaction? added = null;
        _repo
            .AddAsync(Arg.Do<FinancialTransaction>(x => added = x), Arg.Any<CancellationToken>())
            .Returns(ci => Task.FromResult(ci.Arg<FinancialTransaction>()));

        var request = new CreateExpenseFinancialTransactionRequest(
            UserId: User,
            Name: "Expense",
            Amount: 10m,
            OperationDate: _today,
            IsMonthly: true,
            IncomeTransactionId: incomeId
        );

        var result = await _service.CreateExpenseFinancialTransaction(
            request,
            CancellationToken.None
        );

        result.Status.ShouldBe(ResultStatus.Ok);
        result.Value.ShouldBe(added!.Id);
        added.TransactionType.ShouldBe(FinancialTransactionType.Expense);
        added.IncomeTransactionId.ShouldBe(incomeId);
        added.UserId.ShouldBe(User);
        added.Name.ShouldBe("Expense");
        added.Amount.ShouldBe(10m);
        added.IsMonthly.ShouldBeTrue();
        added.OperationDate.ShouldBe(_today);
        await _repo.Received(1).AddAsync(added, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReturnsErrorWhenExpenseDateIsBeforeIncomeDate()
    {
        var incomeId = Guid.NewGuid();
        var income = FinancialTransaction.CreateIncome(
            userId: User,
            name: "Good income",
            amount: 10,
            operationDate: _today.AddDays(1),
            true
        );
        _repo.GetByIdAsync(incomeId, Arg.Any<CancellationToken>()).Returns(income);

        var request = new CreateExpenseFinancialTransactionRequest(
            UserId: User,
            Name: "Expense with bad date",
            Amount: 10m,
            OperationDate: _today,
            IsMonthly: false,
            IncomeTransactionId: incomeId
        );
        var result = await _service.CreateExpenseFinancialTransaction(
            request,
            CancellationToken.None
        );

        result.Status.ShouldBe(ResultStatus.Error);
        result.Errors.ShouldContain(
            "Expense operation date must be greater or equal than income operation date."
        );
        await _repo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task ReturnsOkWhenExpenseDateIsEqualIncomeDate()
    {
        var incomeId = Guid.NewGuid();
        var income = FinancialTransaction.CreateIncome(
            userId: User,
            name: "Good income",
            amount: 10,
            operationDate: _today,
            true
        );
        _repo.GetByIdAsync(incomeId, Arg.Any<CancellationToken>()).Returns(income);

        var request = new CreateExpenseFinancialTransactionRequest(
            UserId: User,
            Name: "Good expense",
            Amount: 10m,
            OperationDate: _today,
            IsMonthly: false,
            IncomeTransactionId: incomeId
        );
        var result = await _service.CreateExpenseFinancialTransaction(
            request,
            CancellationToken.None
        );

        result.Status.ShouldBe(ResultStatus.Ok);
    }

    [Fact]
    public async Task ReturnsOkWhenExpenseDateIsGreaterIncomeDate()
    {
        var incomeId = Guid.NewGuid();
        var income = FinancialTransaction.CreateIncome(
            userId: User,
            name: "Good income",
            amount: 10,
            operationDate: _today,
            true
        );
        _repo.GetByIdAsync(incomeId, Arg.Any<CancellationToken>()).Returns(income);

        var request = new CreateExpenseFinancialTransactionRequest(
            UserId: User,
            Name: "Good expense",
            Amount: 10m,
            OperationDate: _today.AddDays(1),
            IsMonthly: false,
            IncomeTransactionId: incomeId
        );
        var result = await _service.CreateExpenseFinancialTransaction(
            request,
            CancellationToken.None
        );

        result.Status.ShouldBe(ResultStatus.Ok);
    }
}
