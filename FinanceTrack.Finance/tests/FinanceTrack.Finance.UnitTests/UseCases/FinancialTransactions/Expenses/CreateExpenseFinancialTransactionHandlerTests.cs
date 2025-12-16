using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.UseCases.FinancialTransactions.Expenses.Create;

namespace FinanceTrack.Finance.UnitTests.UseCases.FinancialTransactions.Expenses;

public class CreateExpenseFinancialTransactionHandlerTests
{
    private readonly ICreateExpenseFinancialTransactionService _service = Substitute.For<
        ICreateExpenseFinancialTransactionService
    >();

    private readonly CreateExpenseFinancialTransactionHandler _handler;

    public CreateExpenseFinancialTransactionHandlerTests()
    {
        _handler = new CreateExpenseFinancialTransactionHandler(_service);
    }

    [Fact]
    public async Task ReturnsIdOnSuccess()
    {
        var id = Guid.NewGuid();
        _service
            .CreateExpenseFinancialTransaction(Arg.Any<CreateExpenseFinancialTransactionRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(id));

        var command = new CreateExpenseFinancialTransactionCommand(
            UserId: "user",
            Name: "Expense",
            Amount: 10m,
            OperationDate: DateOnly.FromDateTime(DateTime.UtcNow.Date),
            IsMonthly: false,
            IncomeTransactionId: Guid.NewGuid()
        );

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.Ok);
        result.Value.ShouldBe(id);
    }

    [Fact]
    public async Task ForwardsNotFound()
    {
        _service
            .CreateExpenseFinancialTransaction(Arg.Any<CreateExpenseFinancialTransactionRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<Guid>.NotFound());

        var command = new CreateExpenseFinancialTransactionCommand(
            UserId: "user",
            Name: "Expense",
            Amount: 10m,
            OperationDate: DateOnly.FromDateTime(DateTime.UtcNow.Date),
            IsMonthly: false,
            IncomeTransactionId: Guid.NewGuid()
        );

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task ForwardsError()
    {
        _service
            .CreateExpenseFinancialTransaction(Arg.Any<CreateExpenseFinancialTransactionRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<Guid>.Error("err"));

        var command = new CreateExpenseFinancialTransactionCommand(
            UserId: "user",
            Name: "Expense",
            Amount: 10m,
            OperationDate: DateOnly.FromDateTime(DateTime.UtcNow.Date),
            IsMonthly: false,
            IncomeTransactionId: Guid.NewGuid()
        );

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.Error);
        result.Errors.ShouldContain("err");
    }
}

