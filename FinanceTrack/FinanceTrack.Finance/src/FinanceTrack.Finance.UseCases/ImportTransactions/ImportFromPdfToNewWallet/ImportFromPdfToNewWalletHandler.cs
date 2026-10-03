using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.Services;

namespace FinanceTrack.Finance.UseCases.ImportTransactions.ImportFromPdfToNewWallet;

public sealed class ImportFromPdfToNewWalletHandler(
    IBankStatementImportService bankStatementImportService,
    ImportTransactionsService importTransactionsService,
    CreateWalletService createWalletService,
    IUnitOfWork unitOfWork
) : ICommandHandler<ImportFromPdfToNewWalletCommand, Result<ImportFromPdfToNewWalletResult>>
{
    public async Task<Result<ImportFromPdfToNewWalletResult>> Handle(
        ImportFromPdfToNewWalletCommand request,
        CancellationToken cancel
    )
    {
        if (
            !BankStatementType.TryFromName(
                request.BankStatementType,
                ignoreCase: true,
                out var statementType
            )
        )
            return Result<ImportFromPdfToNewWalletResult>.Error("Unknown bank statement type.");

        var parseResult = bankStatementImportService.Import(request.PdfBytes, statementType);
        if (!parseResult.IsSuccess)
            return Result<ImportFromPdfToNewWalletResult>.Error(
                parseResult.Errors.FirstOrDefault() ?? "Failed to parse PDF."
            );

        var walletResult = await createWalletService.Execute(
            new CreateWalletRequest(
                request.UserId,
                request.WalletName,
                request.WalletType,
                request.AllowNegativeBalance,
                request.TargetAmount,
                request.TargetDate
            ),
            cancel
        );
        if (!walletResult.IsSuccess)
            return Result<ImportFromPdfToNewWalletResult>.Error(
                walletResult.Errors.FirstOrDefault() ?? "Failed to create wallet."
            );

        await unitOfWork.SaveChangesAsync(cancel);

        var importResult = await importTransactionsService.ImportAsync(
            parseResult.Value,
            walletResult.Value,
            request.UserId,
            cancel
        );
        if (!importResult.IsSuccess)
            return Result<ImportFromPdfToNewWalletResult>.Error(
                importResult.Errors.FirstOrDefault() ?? "Failed to import transactions."
            );

        await unitOfWork.SaveChangesAsync(cancel);
        return Result.Success(
            new ImportFromPdfToNewWalletResult(walletResult.Value, importResult.Value)
        );
    }
}
