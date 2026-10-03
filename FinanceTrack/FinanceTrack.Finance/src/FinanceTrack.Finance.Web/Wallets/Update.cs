using FinanceTrack.Finance.UseCases.Wallets.Update;
using FinanceTrack.Finance.Web.Extensions;
using FluentValidation;

namespace FinanceTrack.Finance.Web.Wallets;

public class UpdateWalletRequest
{
    public const string Route = "/Wallets/{WalletId:guid}";
    public Guid WalletId { get; set; }
    public string Name { get; set; } = null!;
    public bool AllowNegativeBalance { get; set; }
    public decimal? TargetAmount { get; set; }
    public DateOnly? TargetDate { get; set; }
}

public class UpdateWalletValidator : Validator<UpdateWalletRequest>
{
    public UpdateWalletValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
    }
}

public class UpdateWallet(IMediator mediator) : Endpoint<UpdateWalletRequest, WalletRecord>
{
    public override void Configure()
    {
        Put(UpdateWalletRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(UpdateWalletRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var command = new UpdateWalletCommand(
            req.WalletId,
            userId,
            req.Name,
            req.AllowNegativeBalance,
            req.TargetAmount,
            req.TargetDate
        );

        var result = await mediator.Send(command, ct);

        if (await this.SendResultIfNotOk(result, ct))
            return;

        var w = result.Value;
        Response = new WalletRecord(
            w.Id, w.Name, w.WalletType, w.Balance,
            w.AllowNegativeBalance, w.TargetAmount, w.TargetDate, w.IsArchived
        );
    }
}
