using FinanceTrack.Finance.Infrastructure.Data.Config;
using FinanceTrack.Finance.UseCases.FinancialTransactions.Transfer;
using FinanceTrack.Finance.Web.Extensions;
using FluentValidation;

namespace FinanceTrack.Finance.Web.Transactions;

public class CreateTransferRequest
{
    public const string Route = "/Transactions/Transfer";
    public Guid FromWalletId { get; set; }
    public Guid ToWalletId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateOnly OperationDate { get; set; }
}

public class CreateTransferResponse(Guid id)
{
    public Guid Id { get; set; } = id;
}

public class CreateTransferValidator : Validator<CreateTransferRequest>
{
    public CreateTransferValidator()
    {
        RuleFor(x => x.FromWalletId)
            .Must(id => id != Guid.Empty)
            .WithMessage("FromWalletId is required.");
        RuleFor(x => x.ToWalletId)
            .Must(id => id != Guid.Empty)
            .WithMessage("ToWalletId is required.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Description)
            .MaximumLength(
                FinancialTransactionDataSchemaConstants.TRANSACTION_DESCRIPTION_MAX_LENGTH
            )
            .WithMessage("Description length must be less than 501");
        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0.01m)
            .WithMessage("Amount must be at least 0.01.");
        RuleFor(x => x.OperationDate)
            .Must(d => d != default)
            .WithMessage("OperationDate is required.");
    }
}

public class CreateTransfer(IMediator mediator)
    : Endpoint<CreateTransferRequest, CreateTransferResponse>
{
    public override void Configure()
    {
        Post(CreateTransferRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(CreateTransferRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var command = new CreateTransferCommand(
            userId,
            req.FromWalletId,
            req.ToWalletId,
            req.Name,
            req.Description,
            req.Amount,
            req.OperationDate
        );
        var result = await mediator.Send(command, ct);

        if (await this.SendResultIfNotOk(result, ct))
            return;

        await SendAsync(new CreateTransferResponse(result.Value), 201, ct);
    }
}
