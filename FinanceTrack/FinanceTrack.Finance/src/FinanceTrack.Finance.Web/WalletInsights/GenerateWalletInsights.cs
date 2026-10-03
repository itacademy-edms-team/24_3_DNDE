using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.UseCases.WalletInsights;
using FinanceTrack.Finance.UseCases.WalletInsights.Generate;
using FinanceTrack.Finance.Web.Extensions;
using FluentValidation;

namespace FinanceTrack.Finance.Web.WalletInsights;

public class GenerateWalletInsightsRequest
{
    public const string Route = "/Wallets/{WalletId:guid}/Insights/Generate";

    public Guid WalletId { get; set; }
}

public class GenerateWalletInsightsValidator : Validator<GenerateWalletInsightsRequest>
{
    public GenerateWalletInsightsValidator()
    {
        RuleFor(r => r.WalletId).NotEmpty().WithMessage("WalletId is required.");
    }
}

public class GenerateWalletInsights(IMediator mediator, IWalletInsightsAiService aiService)
    : Endpoint<GenerateWalletInsightsRequest, WalletInsightDto>
{
    public override void Configure()
    {
        Post(GenerateWalletInsightsRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(
        GenerateWalletInsightsRequest req,
        CancellationToken cancel
    )
    {
        if (!aiService.IsEnabled)
        {
            AddError("AI insights generation is not enabled.");
            await SendErrorsAsync(503, cancel);
            return;
        }

        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(cancel);
            return;
        }

        var result = await mediator.Send(
            new GenerateWalletInsightsCommand(userId, req.WalletId),
            cancel
        );

        if (await this.SendResultIfNotOk(result, cancel))
            return;

        await SendOkAsync(result.Value, cancel);
    }
}
