using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.UseCases.FinancialTransactions.Expenses.Update;

namespace FinanceTrack.Finance.UnitTests.UseCases.FinancialTransactions.Expenses;

public class UpdateExpenseFinancialTransactionHandlerTests
{
    private const string User = "user";
    private readonly IUpdateExpenseFinancialTransactionService _service =
        Substitute.For<IUpdateExpenseFinancialTransactionService>();

    private readonly UpdateExpenseFinancialTransactionHandler _handler;
    private readonly DateOnly _today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

    public UpdateExpenseFinancialTransactionHandlerTests()
    {
        _handler = new UpdateExpenseFinancialTransactionHandler(_service);
    }

    [Fact]
    public async Task ReturnsDtoOnSuccess()
    {
        var incomeId = Guid.NewGuid();
        var expense = FinancialTransaction.CreateExpense(
            userId: "user",
            name: "Expense",
            amount: 10m,
            operationDate: _today,
            isMonthly: true,
            incomeTransactionId: incomeId
        );

        _service
            .UpdateExpenseFinancialTransaction(
                Arg.Any<UpdateExpenseFinancialTransactionRequest>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Result.Success(expense));

        var command = new UpdateExpenseFinancialTransactionCommand(
            TransactionId: expense.Id,
            UserId: User,
            Name: "New",
            Amount: 12.34m,
            OperationDate: _today.AddDays(1),
            IsMonthly: true,
            IncomeTransactionId: incomeId
        );

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.Ok);
        var dto = result.Value;
        dto.Id.ShouldBe(expense.Id);
        dto.Type.ShouldBe(expense.TransactionType.Name);
    }

    [Fact]
    public async Task ForwardsNotFound()
    {
        _service
            .UpdateExpenseFinancialTransaction(
                Arg.Any<UpdateExpenseFinancialTransactionRequest>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Result<FinancialTransaction>.NotFound());

        var command = new UpdateExpenseFinancialTransactionCommand(
            TransactionId: Guid.NewGuid(),
            UserId: User,
            Name: "New",
            Amount: 12.34m,
            OperationDate: _today,
            IsMonthly: true,
            IncomeTransactionId: Guid.NewGuid()
        );

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.NotFound);
    }
}
