using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.WalletAggregate;
using FinanceTrack.Finance.UseCases.Wallets.Create;

namespace FinanceTrack.Finance.UnitTests.UseCases.Wallets.Create;

public class CreateWalletHandlerHandle
{
    private readonly IRepository<Wallet> _repo = Substitute.For<IRepository<Wallet>>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateWalletHandler _handler;

    private const string UserId = "user-1";

    public CreateWalletHandlerHandle()
    {
        _handler = new CreateWalletHandler(_repo, _unitOfWork);
    }

    [Fact]
    public async Task Handle_CheckingWallet_SuccessAndPersists()
    {
        var command = new CreateWalletCommand(
            UserId: UserId,
            Name: "Main",
            WalletType: "Checking",
            AllowNegativeBalance: false,
            TargetAmount: null,
            TargetDate: null
        );

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBe(Guid.Empty);

        await _repo
            .Received(1)
            .AddAsync(Arg.Any<Wallet>(), Arg.Any<CancellationToken>());
        await _unitOfWork
            .Received(1)
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SavingsWithoutTargetAmount_ReturnsErrorAndDoesNotPersist()
    {
        var command = new CreateWalletCommand(
            UserId: UserId,
            Name: "Savings",
            WalletType: "Savings",
            AllowNegativeBalance: false,
            TargetAmount: null,
            TargetDate: null
        );

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.Error);

        await _repo
            .DidNotReceive()
            .AddAsync(Arg.Any<Wallet>(), Arg.Any<CancellationToken>());
        await _unitOfWork
            .DidNotReceive()
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

