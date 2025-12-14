using FinanceTrack.Finance.UseCases.FinancialTransactions;
using FinanceTrack.Finance.UseCases.FinancialTransactions.Expenses.List;

namespace FinanceTrack.Finance.UnitTests.UseCases.FinancialTransactions.Expenses;

public class ListUserExpenseFinancialTransactionsHandlerTests
{
    private readonly IListUserExpenseFinancialTransactionsQueryService _service = Substitute.For<
        IListUserExpenseFinancialTransactionsQueryService
    >();

    private readonly ListUserExpenseFinancialTransactionsHandler _handler;

    public ListUserExpenseFinancialTransactionsHandlerTests()
    {
        _handler = new ListUserExpenseFinancialTransactionsHandler(_service);
    }

    [Fact]
    public async Task ReturnsListOnSuccess()
    {
        var incomeId = Guid.NewGuid();
        var dto = new FinancialTransactionDto(
            Id: Guid.NewGuid(),
            Name: "Expense",
            Amount: 10m,
            OperationDate: DateOnly.FromDateTime(DateTime.UtcNow.Date),
            IsMonthly: false,
            Type: "Expense"
        );

        _service
            .GetUserExpenseTransactions("user", incomeId, Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<FinancialTransactionDto>>(new[] { dto }));

        var result = await _handler.Handle(
            new ListUserExpenseFinancialTransactionsQuery("user", incomeId),
            CancellationToken.None
        );

        result.Status.ShouldBe(ResultStatus.Ok);
        result.Value.ShouldHaveSingleItem().ShouldBe(dto);
    }

    [Fact]
    public async Task ForwardsNotFound()
    {
        var incomeId = Guid.NewGuid();
        _service
            .GetUserExpenseTransactions("user", incomeId, Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<FinancialTransactionDto>>.NotFound());

        var result = await _handler.Handle(
            new ListUserExpenseFinancialTransactionsQuery("user", incomeId),
            CancellationToken.None
        );

        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task ForwardsForbidden()
    {
        var incomeId = Guid.NewGuid();
        _service
            .GetUserExpenseTransactions("user", incomeId, Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<FinancialTransactionDto>>.Forbidden());

        var result = await _handler.Handle(
            new ListUserExpenseFinancialTransactionsQuery("user", incomeId),
            CancellationToken.None
        );

        result.Status.ShouldBe(ResultStatus.Forbidden);
    }

    [Fact]
    public async Task ForwardsError()
    {
        var incomeId = Guid.NewGuid();
        _service
            .GetUserExpenseTransactions("user", incomeId, Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<FinancialTransactionDto>>.Error("boom"));

        var result = await _handler.Handle(
            new ListUserExpenseFinancialTransactionsQuery("user", incomeId),
            CancellationToken.None
        );

        result.Status.ShouldBe(ResultStatus.Error);
        result.Errors.ShouldContain("boom");
    }
}

