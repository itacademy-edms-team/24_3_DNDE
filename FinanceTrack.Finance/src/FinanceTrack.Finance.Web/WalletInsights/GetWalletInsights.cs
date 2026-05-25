using FinanceTrack.Finance.UseCases.WalletInsights;
using FinanceTrack.Finance.UseCases.WalletInsights.Get;
using FinanceTrack.Finance.Web.Extensions;
using FluentValidation;

namespace FinanceTrack.Finance.Web.WalletInsights;

public class GetWalletInsightsRequest
{
    public const string Route = "/Wallets/{WalletId:guid}/Insights";

    public Guid WalletId { get; set; }

    [QueryParam]
    public DateOnly? InsightMonth { get; set; }
}

public class GetWalletInsightsValidator : Validator<GetWalletInsightsRequest>
{
    public GetWalletInsightsValidator()
    {
        RuleFor(r => r.WalletId).NotEmpty().WithMessage("WalletId is required.");
        RuleFor(r => r.InsightMonth)
            .Must(d => d is not null && d.Value <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("InsightMonth cannot be in the future.");
    }
}

public class GetWalletInsights(IMediator mediator)
    : Endpoint<GetWalletInsightsRequest, WalletInsightDto>
{
    public override void Configure()
    {
        Get(GetWalletInsightsRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(GetWalletInsightsRequest req, CancellationToken cancel)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(cancel);
            return;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var insightMonth = req.InsightMonth ?? new DateOnly(today.Year, today.Month, 1);

        var result = await mediator.Send(
            new GetWalletInsightsQuery(userId, req.WalletId, insightMonth),
            cancel
        );

        if (await this.SendResultIfNotOk(result, cancel))
            return;

        await SendOkAsync(result.Value, cancel);
    }
}
