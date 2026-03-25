using FinanceTrack.Finance.UseCases.Analytics.Dto;
using FinanceTrack.Finance.UseCases.Analytics.General;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.Analytics.General;

public class GetOverviewRequest
{
    public const string Route = "/Analytics/Overview";

    [QueryParam]
    public DateOnly From { get; set; }

    [QueryParam]
    public DateOnly To { get; set; }
}

public class GetOverview(IMediator mediator) : Endpoint<GetOverviewRequest, OverviewAnalyticsDto>
{
    public override void Configure()
    {
        Get(GetOverviewRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(GetOverviewRequest req, CancellationToken cancel)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(cancel);
            return;
        }

        var result = await mediator.Send(
            new GetGeneralOverviewQuery(userId, req.From, req.To),
            cancel
        );
        await SendOkAsync(result, cancel);
    }
}
