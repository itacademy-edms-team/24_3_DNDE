using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.Services;
using FinanceTrack.Finance.Core.WalletAggregate;

namespace FinanceTrack.Finance.UnitTests.Core.Services;

public class TransferServiceTests
{
    private readonly IRepository<FinancialTransaction> _transactionRepo =
        Substitute.For<IRepository<FinancialTransaction>>();
    private readonly IRepository<Wallet> _walletRepo =
        Substitute.For<IRepository<Wallet>>();
    private readonly TransferService _sut;

    private const string UserId = "user-1";
    private static readonly Guid FromWalletId = Guid.NewGuid();
    private static readonly Guid ToWalletId = Guid.NewGuid();
    private static readonly DateOnly Today = new(2026, 2, 25);

    public TransferServiceTests()
    {
        _sut = new TransferService(_transactionRepo, _walletRepo);
    }

    private Wallet CreateWallet(string userId, decimal balance = 0m, bool allowNeg = true, bool archived = false)
    {
        var w = Wallet.CreateChecking(userId, "Wallet", allowNeg);
        if (balance > 0) w.Credit(balance);
        if (archived) w.Archive();
        return w;
    }

    [Fact]
    public async Task Execute_SameWallet_ReturnsError()
    {
        var sameId = Guid.NewGuid();
        var request = new CreateTransferRequest(UserId, sameId, sameId, "Self-transfer", 100m, Today);
        var result = await _sut.Execute(request);

        result.Status.ShouldBe(ResultStatus.Error);
        result.Errors.ShouldContain("Cannot transfer to the same wallet.");
    }

    [Fact]
    public async Task Execute_SourceWalletNotFound_ReturnsNotFound()
    {
        _walletRepo.GetByIdAsync(FromWalletId, Arg.Any<CancellationToken>())
            .Returns((Wallet?)null);

        var request = new CreateTransferRequest(UserId, FromWalletId, ToWalletId, "Transfer", 100m, Today);
        var result = await _sut.Execute(request);

        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Execute_SourceWalletWrongUser_ReturnsForbidden()
    {
        var fromWallet = CreateWallet("other-user", 500m);
        _walletRepo.GetByIdAsync(FromWalletId, Arg.Any<CancellationToken>())
            .Returns(fromWallet);

        var request = new CreateTransferRequest(UserId, FromWalletId, ToWalletId, "Transfer", 100m, Today);
        var result = await _sut.Execute(request);

        result.Status.ShouldBe(ResultStatus.Forbidden);
    }

    [Fact]
    public async Task Execute_SourceWalletArchived_ReturnsError()
    {
        var fromWallet = CreateWallet(UserId, 500m, archived: true);
        _walletRepo.GetByIdAsync(FromWalletId, Arg.Any<CancellationToken>())
            .Returns(fromWallet);

        var request = new CreateTransferRequest(UserId, FromWalletId, ToWalletId, "Transfer", 100m, Today);
        var result = await _sut.Execute(request);

        result.Status.ShouldBe(ResultStatus.Error);
    }

    [Fact]
    public async Task Execute_DestWalletNotFound_ReturnsNotFound()
    {
        var fromWallet = CreateWallet(UserId, 500m);
        _walletRepo.GetByIdAsync(FromWalletId, Arg.Any<CancellationToken>())
            .Returns(fromWallet);
        _walletRepo.GetByIdAsync(ToWalletId, Arg.Any<CancellationToken>())
            .Returns((Wallet?)null);

        var request = new CreateTransferRequest(UserId, FromWalletId, ToWalletId, "Transfer", 100m, Today);
        var result = await _sut.Execute(request);

        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Execute_DestWalletWrongUser_ReturnsForbidden()
    {
        var fromWallet = CreateWallet(UserId, 500m);
        var toWallet = CreateWallet("other-user");
        _walletRepo.GetByIdAsync(FromWalletId, Arg.Any<CancellationToken>())
            .Returns(fromWallet);
        _walletRepo.GetByIdAsync(ToWalletId, Arg.Any<CancellationToken>())
            .Returns(toWallet);

        var request = new CreateTransferRequest(UserId, FromWalletId, ToWalletId, "Transfer", 100m, Today);
        var result = await _sut.Execute(request);

        result.Status.ShouldBe(ResultStatus.Forbidden);
    }

    [Fact]
    public async Task Execute_InsufficientFunds_ReturnsError()
    {
        var fromWallet = CreateWallet(UserId, 30m, allowNeg: false);
        var toWallet = CreateWallet(UserId);
        _walletRepo.GetByIdAsync(FromWalletId, Arg.Any<CancellationToken>())
            .Returns(fromWallet);
        _walletRepo.GetByIdAsync(ToWalletId, Arg.Any<CancellationToken>())
            .Returns(toWallet);

        var request = new CreateTransferRequest(UserId, FromWalletId, ToWalletId, "Transfer", 100m, Today);
        var result = await _sut.Execute(request);

        result.Status.ShouldBe(ResultStatus.Error);
    }

    [Fact]
    public async Task Execute_ValidTransfer_DebitsSourceCreditsDestination()
    {
        var fromWallet = CreateWallet(UserId, 500m);
        var toWallet = CreateWallet(UserId, 100m);
        _walletRepo.GetByIdAsync(FromWalletId, Arg.Any<CancellationToken>())
            .Returns(fromWallet);
        _walletRepo.GetByIdAsync(ToWalletId, Arg.Any<CancellationToken>())
            .Returns(toWallet);

        var request = new CreateTransferRequest(UserId, FromWalletId, ToWalletId, "Transfer", 200m, Today);
        var result = await _sut.Execute(request);

        result.IsSuccess.ShouldBeTrue();
        fromWallet.Balance.ShouldBe(300m);
        toWallet.Balance.ShouldBe(300m);
    }

    [Fact]
    public async Task Execute_ValidTransfer_CreatesTwoTransactions()
    {
        var fromWallet = CreateWallet(UserId, 500m);
        var toWallet = CreateWallet(UserId, 100m);
        _walletRepo.GetByIdAsync(FromWalletId, Arg.Any<CancellationToken>())
            .Returns(fromWallet);
        _walletRepo.GetByIdAsync(ToWalletId, Arg.Any<CancellationToken>())
            .Returns(toWallet);

        var request = new CreateTransferRequest(UserId, FromWalletId, ToWalletId, "Transfer", 200m, Today);
        await _sut.Execute(request);

        // Should add two transactions (TransferOut + TransferIn)
        await _transactionRepo.Received(2)
            .AddAsync(Arg.Any<FinancialTransaction>(), Arg.Any<CancellationToken>());
        // Should update both wallets
        await _walletRepo.Received(1).UpdateAsync(fromWallet, Arg.Any<CancellationToken>());
        await _walletRepo.Received(1).UpdateAsync(toWallet, Arg.Any<CancellationToken>());
    }
}
