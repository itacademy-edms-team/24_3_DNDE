using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.Services;
using FinanceTrack.Finance.UseCases.FinancialTransactions.Expense;

namespace FinanceTrack.Finance.UnitTests.UseCases.FinancialTransactions.Expenses;

public class CreateExpenseHandlerHandle
{
    private readonly CreateExpenseService _service;
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateExpenseHandler _handler;

    private const string UserId = "user-1";

    public CreateExpenseHandlerHandle()
    {
        var transactionRepo = Substitute.For<IRepository<FinanceTrack.Finance.Core.FinancialTransactionAggregate.FinancialTransaction>>();
        var walletRepo = Substitute.For<IRepository<FinanceTrack.Finance.Core.WalletAggregate.Wallet>>();

        _service = new CreateExpenseService(transactionRepo, walletRepo);
        _handler = new CreateExpenseHandler(_service, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenServiceReturnsError_DoesNotCallUnitOfWork()
    {
        var command = new CreateExpenseCommand(
            UserId: UserId,
            WalletId: Guid.NewGuid(),
            Name: "Test",
            Amount: 10m,
            OperationDate: new DateOnly(2026, 2, 27),
            CategoryId: null
        );

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        await _unitOfWork
            .DidNotReceive()
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

