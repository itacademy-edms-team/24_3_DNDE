using FinanceTrack.Finance.UseCases.ImportTransactions;
using FinanceTrack.Finance.UseCases.ImportTransactions.ImportFromPdfToNewWallet;
using FinanceTrack.Finance.Web.Extensions;
using FinanceTrack.Finance.Web.Wallets;
using FluentValidation;
using MediatR;

namespace FinanceTrack.Finance.Web.FinancialTransactions;

public class ImportFromPdfToNewWalletRequest
{
    public const string Route = "/Transactions/Import/Pdf/NewWallet";
    public string BankStatementType { get; set; } = null!;
    public IFormFile PdfFile { get; set; } = null!;
    public string WalletName { get; set; } = null!;
    public string WalletType { get; set; } = null!;
    public bool AllowNegativeBalance { get; set; } = true;
    public decimal? TargetAmount { get; set; }
    public DateOnly? TargetDate { get; set; }
}

public class ImportFromPdfToNewWalletResponse(Guid walletId, int importCount)
{
    public Guid WalletId { get; set; } = walletId;

    public int ImportCount { get; set; } = importCount;
}

public class ImportFromPdfToNewWalletValidator : Validator<ImportFromPdfToNewWalletRequest>
{
    public ImportFromPdfToNewWalletValidator()
    {
        RuleFor(x => x.BankStatementType)
            .Must(value => BankStatementType.TryFromName(value, ignoreCase: true, out _))
            .WithMessage("Unknown bank statement type.");
        RuleFor(x => x.PdfFile).NotNull().WithMessage("File is required");
        RuleFor(x => x.PdfFile.Length)
            .LessThanOrEqualTo(10 * 1024 * 1024)
            .WithMessage("File size must be less than or equal to 10 MB.");
        RuleFor(x => x.PdfFile.ContentType)
            .Equal("application/pdf")
            .WithMessage("Content type miss match. File must be an application/pdf");
        RuleFor(x => x.WalletName).NotEmpty().WithMessage("WalletName is required.");
        RuleFor(x => x.WalletType).NotEmpty().WithMessage("WalletType is required.");
    }
}

public class ImportFromPdfToNewWallet(IMediator mediator)
    : Endpoint<ImportFromPdfToNewWalletRequest, ImportFromPdfToNewWalletResponse>
{
    public override void Configure()
    {
        Post(ImportFromPdfToNewWalletRequest.Route);
        Roles("user");
        AllowFileUploads();
    }

    public override async Task HandleAsync(
        ImportFromPdfToNewWalletRequest req,
        CancellationToken cancel
    )
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(cancel);
            return;
        }

        using var ms = new MemoryStream();
        await req.PdfFile.CopyToAsync(ms, cancel);

        var command = new ImportFromPdfToNewWalletCommand(
            userId,
            req.BankStatementType,
            ms.ToArray(),
            req.WalletName,
            req.WalletType,
            req.AllowNegativeBalance,
            req.TargetAmount,
            req.TargetDate
        );
        var result = await mediator.Send(command, cancel);

        if (await this.SendResultIfNotOk(result, cancel))
            return;

        await SendCreatedAtAsync<GetWallet>(
            new { id = result.Value },
            new ImportFromPdfToNewWalletResponse(result.Value.WalletId, result.Value.ImportCount),
            cancellation: cancel
        );
    }
}
