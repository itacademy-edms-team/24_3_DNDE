using FinanceTrack.Finance.UseCases.Analytics;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.Analytics;

public class GetCategoriesAnalyticsRequest
{
    public const string Route = "/Analytics/Categories";
    [QueryParam] public DateOnly From { get; set; }
    [QueryParam] public DateOnly To { get; set; }
}

public class GetCategoriesAnalytics(IMediator mediator)
    : Endpoint<GetCategoriesAnalyticsRequest, CategoriesAnalyticsDto>
{
    public override void Configure()
    {
        Get(GetCategoriesAnalyticsRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(GetCategoriesAnalyticsRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var result = await mediator.Send(
            new GetCategoriesAnalyticsQuery(userId, req.From, req.To),
            ct
        );
        await SendOkAsync(result, ct);
    }
}
