using FinanceTrack.Finance.UseCases.Wallets.Create;
using FinanceTrack.Finance.Web.Extensions;
using FluentValidation;

namespace FinanceTrack.Finance.Web.Wallets;

public class CreateWalletRequest
{
    public const string Route = "/Wallets";
    public string Name { get; set; } = null!;
    public string WalletType { get; set; } = null!; // "Checking" or "Savings"
    public bool AllowNegativeBalance { get; set; } = true;
    public decimal? TargetAmount { get; set; }
    public DateOnly? TargetDate { get; set; }
}

public class CreateWalletResponse(Guid id)
{
    public Guid Id { get; set; } = id;
}

public class CreateWalletValidator : Validator<CreateWalletRequest>
{
    public CreateWalletValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.WalletType).NotEmpty().WithMessage("WalletType is required.");
    }
}

public class Create(IMediator mediator) : Endpoint<CreateWalletRequest, CreateWalletResponse>
{
    public override void Configure()
    {
        Post(CreateWalletRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(CreateWalletRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var command = new CreateWalletCommand(
            userId,
            req.Name,
            req.WalletType,
            req.AllowNegativeBalance,
            req.TargetAmount,
            req.TargetDate
        );

        var result = await mediator.Send(command, ct);

        if (await this.SendResultIfNotOk(result, ct))
            return;

        await SendCreatedAtAsync<Create>(new { id = result.Value }, new CreateWalletResponse(result.Value), cancellation: ct);
    }
}
