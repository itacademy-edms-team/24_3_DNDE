using FinanceTrack.Finance.UseCases.Analytics.Dto;
using FinanceTrack.Finance.UseCases.Analytics.General;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.Analytics.General;

public class GetCategoriesAnalyticsRequest
{
    public const string Route = "/Analytics/Categories";

    [QueryParam]
    public DateOnly From { get; set; }

    [QueryParam]
    public DateOnly To { get; set; }
}

public class GetCategoriesAnalytics(IMediator mediator)
    : Endpoint<GetCategoriesAnalyticsRequest, CategoriesAnalyticsDto>
{
    public override void Configure()
    {
        Get(GetCategoriesAnalyticsRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(
        GetCategoriesAnalyticsRequest req,
        CancellationToken cancel
    )
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(cancel);
            return;
        }

        var result = await mediator.Send(
            new GetGeneralCategoriesAnalyticsQuery(userId, req.From, req.To),
            cancel
        );
        await SendOkAsync(result, cancel);
    }
}
