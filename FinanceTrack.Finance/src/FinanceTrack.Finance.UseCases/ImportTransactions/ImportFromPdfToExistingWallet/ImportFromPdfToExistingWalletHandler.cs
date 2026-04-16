namespace FinanceTrack.Finance.UseCases.ImportTransactions.ImportFromPdfToExistingWallet;

public sealed class ImportFromPdfToExistingWalletHandler(
    IBankStatementImportService bankStatementImportService,
    ImportTransactionsService importTransactionsService,
    IUnitOfWork unitOfWork
) : ICommandHandler<ImportFromPdfToExistingWalletCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        ImportFromPdfToExistingWalletCommand request,
        CancellationToken cancel
    )
    {
        if (!BankStatementType.TryFromName(request.BankStatementType, ignoreCase: true, out var statementType))
            return Result<int>.Error("Unknown bank statement type.");

        var parseResult = bankStatementImportService.Import(request.PdfBytes, statementType);
        if (!parseResult.IsSuccess)
            return Result<int>.Error(parseResult.Errors.FirstOrDefault() ?? "Failed to parse PDF.");

        var importResult = await importTransactionsService.ImportAsync(
            parseResult.Value,
            request.WalletId,
            request.UserId,
            cancel
        );
        if (!importResult.IsSuccess)
            return Result<int>.Error(importResult.Errors.FirstOrDefault() ?? "Failed to import transactions.");

        await unitOfWork.SaveChangesAsync(cancel);
        return Result.Success(importResult.Value);
    }
}
