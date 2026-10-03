using FinanceTrack.Finance.UseCases.ImportTransactions;
using FinanceTrack.Finance.UseCases.ImportTransactions.ImportFromPdfToExistingWallet;
using FinanceTrack.Finance.Web.Extensions;
using FinanceTrack.Finance.Web.Transactions;
using FluentValidation;

namespace FinanceTrack.Finance.Web.FinancialTransactions;

public class ImportFromPdfToExistingWalletRequest
{
    public const string Route = "/Transactions/Import/Pdf/ExistingWallet";
    public Guid WalletId { get; set; }
    public string BankStatementType { get; set; } = null!;
    public IFormFile PdfFile { get; set; } = null!;
}

public class ImportFromPdfToExistingWalletResponse(int importCount)
{
    public int ImportCount { get; set; } = importCount;
}

public class ImportFromPdfToExistingWalletValidator
    : Validator<ImportFromPdfToExistingWalletRequest>
{
    public ImportFromPdfToExistingWalletValidator()
    {
        RuleFor(x => x.WalletId).NotEmpty().WithMessage("Incorrect WalletId.");
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
    }
}

public class ImportFromPdfToExistingWallet(IMediator mediator)
    : Endpoint<ImportFromPdfToExistingWalletRequest, ImportFromPdfToExistingWalletResponse>
{
    public override void Configure()
    {
        Post(ImportFromPdfToExistingWalletRequest.Route);
        Roles("user");
        AllowFileUploads();
    }

    public override async Task HandleAsync(
        ImportFromPdfToExistingWalletRequest req,
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

        var command = new ImportFromPdfToExistingWalletCommand(
            userId,
            req.WalletId,
            req.BankStatementType,
            ms.ToArray()
        );
        var result = await mediator.Send(command, cancel);

        if (await this.SendResultIfNotOk(result, cancel))
            return;

        await SendOkAsync(new ImportFromPdfToExistingWalletResponse(result.Value), cancel);
    }
}
