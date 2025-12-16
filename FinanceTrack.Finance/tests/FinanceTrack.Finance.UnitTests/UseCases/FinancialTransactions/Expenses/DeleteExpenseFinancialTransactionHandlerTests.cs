using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.UseCases.FinancialTransactions.Expenses.Delete;

namespace FinanceTrack.Finance.UnitTests.UseCases.FinancialTransactions.Expenses;

public class DeleteExpenseFinancialTransactionHandlerTests
{
    private readonly IDeleteExpenseFinancialTransactionService _service = Substitute.For<
        IDeleteExpenseFinancialTransactionService
    >();

    private readonly DeleteExpenseFinancialTransactionHandler _handler;

    public DeleteExpenseFinancialTransactionHandlerTests()
    {
        _handler = new DeleteExpenseFinancialTransactionHandler(_service);
    }

    [Fact]
    public async Task ReturnsOkOnSuccess()
    {
        _service
            .DeleteExpenseFinancialTransaction(
                Arg.Any<DeleteExpenseFinancialTransactionRequest>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Result.Success());

        var command = new DeleteExpenseFinancialTransactionCommand(
            TransactionId: Guid.NewGuid(),
            UserId: "user"
        );

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.Ok);
    }

    [Fact]
    public async Task ForwardsForbidden()
    {
        _service
            .DeleteExpenseFinancialTransaction(
                Arg.Any<DeleteExpenseFinancialTransactionRequest>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Result.Forbidden());

        var command = new DeleteExpenseFinancialTransactionCommand(
            TransactionId: Guid.NewGuid(),
            UserId: "user"
        );

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.Forbidden);
    }

    [Fact]
    public async Task ForwardsNotFound()
    {
        _service
            .DeleteExpenseFinancialTransaction(
                Arg.Any<DeleteExpenseFinancialTransactionRequest>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Result.NotFound());

        var command = new DeleteExpenseFinancialTransactionCommand(
            TransactionId: Guid.NewGuid(),
            UserId: "user"
        );

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.NotFound);
    }
}

