using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.Services;
using FinanceTrack.Finance.Core.WalletAggregate;
using FinanceTrack.Finance.IntegrationTests.Data;

namespace FinanceTrack.Finance.IntegrationTests.Services;

public class TransferServiceTests : BaseEfRepoTestFixture
{
    private const string UserId = "user-1";
    private static readonly DateOnly Today = new(2026, 2, 25);

    [Fact]
    public async Task Execute_ValidTransfer_MovesBalanceAndCreatesTwoTransactions()
    {
        var walletRepo = GetWalletRepository();
        var transactionRepo = GetFinancialTransactionRepository();

        // Create two wallets
        var from = Wallet.CreateChecking(UserId, "From Wallet");
        from.Credit(1000m);
        var to = Wallet.CreateChecking(UserId, "To Wallet");

        await walletRepo.AddAsync(from);
        await walletRepo.AddAsync(to);

        var service = new TransferService(transactionRepo, walletRepo);
        var request = new CreateTransferRequest(UserId, from.Id, to.Id, "Transfer", 300m, Today);

        var result = await service.Execute(request);

        result.IsSuccess.ShouldBeTrue();

        // Verify balances
        var updatedFrom = await walletRepo.GetByIdAsync(from.Id);
        var updatedTo = await walletRepo.GetByIdAsync(to.Id);
        updatedFrom!.Balance.ShouldBe(700m);
        updatedTo!.Balance.ShouldBe(300m);

        // Verify two transactions created (TransferOut + TransferIn)
        var transactions = await transactionRepo.ListAsync();
        transactions.Count.ShouldBe(2);

        var transferOut = transactions.Single(t =>
            t.TransactionType == FinancialTransactionType.TransferOut
        );
        var transferIn = transactions.Single(t =>
            t.TransactionType == FinancialTransactionType.TransferIn
        );

        transferOut.Amount.ShouldBe(300m);
        transferIn.Amount.ShouldBe(300m);
        transferOut.WalletId.ShouldBe(from.Id);
        transferIn.WalletId.ShouldBe(to.Id);

        // Verify linked via RelatedTransactionId
        transferOut.RelatedTransactionId.ShouldBe(transferIn.Id);
        transferIn.RelatedTransactionId.ShouldBe(transferOut.Id);
    }

    [Fact]
    public async Task Execute_SameWallet_ReturnsError()
    {
        var walletRepo = GetWalletRepository();
        var transactionRepo = GetFinancialTransactionRepository();

        var wallet = Wallet.CreateChecking(UserId, "Wallet");
        await walletRepo.AddAsync(wallet);

        var service = new TransferService(transactionRepo, walletRepo);
        var request = new CreateTransferRequest(UserId, wallet.Id, wallet.Id, "Self", 100m, Today);

        var result = await service.Execute(request);

        result.Status.ShouldBe(Ardalis.Result.ResultStatus.Error);
    }
}
