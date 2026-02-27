using Ardalis.Specification;
using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.RecurringTransactionAggregate;
using FinanceTrack.Finance.Core.WalletAggregate;
using FinanceTrack.Finance.UseCases.RecurringTransactions.Create;

namespace FinanceTrack.Finance.UnitTests.UseCases.RecurringTransactions.Create;

public class CreateRecurringTransactionHandlerHandle
{
    private readonly IRepository<RecurringTransaction> _repo =
        Substitute.For<IRepository<RecurringTransaction>>();
    private readonly IReadRepository<Wallet> _walletRepo =
        Substitute.For<IReadRepository<Wallet>>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateRecurringTransactionHandler _handler;

    private const string UserId = "user-1";
    private static readonly Guid WalletId = Guid.NewGuid();

    public CreateRecurringTransactionHandlerHandle()
    {
        _handler = new CreateRecurringTransactionHandler(_repo, _walletRepo, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WalletNotFound_ReturnsNotFound_AndDoesNotPersist()
    {
        _walletRepo
            .FirstOrDefaultAsync(
                Arg.Any<ISpecification<Wallet>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns((Wallet?)null);

        var command = new CreateRecurringTransactionCommand(
            UserId: UserId,
            WalletId: WalletId,
            Name: "Rent",
            Type: "Expense",
            Amount: 1000m,
            DayOfMonth: 5,
            StartDate: new DateOnly(2026, 2, 1),
            EndDate: null,
            CategoryId: null
        );

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.NotFound);

        await _repo
            .DidNotReceive()
            .AddAsync(Arg.Any<RecurringTransaction>(), Arg.Any<CancellationToken>());
        await _unitOfWork
            .DidNotReceive()
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidIncomeRule_PersistsAndReturnsSuccess()
    {
        var wallet = Wallet.CreateChecking(UserId, "Main");

        _walletRepo
            .FirstOrDefaultAsync(
                Arg.Any<ISpecification<Wallet>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(wallet);

        var command = new CreateRecurringTransactionCommand(
            UserId: UserId,
            WalletId: wallet.Id,
            Name: "Salary",
            Type: "Income",
            Amount: 3000m,
            DayOfMonth: 1,
            StartDate: new DateOnly(2026, 2, 1),
            EndDate: null,
            CategoryId: null
        );

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBe(Guid.Empty);

        await _repo
            .Received(1)
            .AddAsync(Arg.Any<RecurringTransaction>(), Arg.Any<CancellationToken>());
        await _unitOfWork
            .Received(1)
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

