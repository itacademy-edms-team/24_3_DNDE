using FinanceTrack.Finance.UseCases.Analytics;
using FinanceTrack.Finance.UseCases.Analytics.Dto;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.Analytics.General;

public class GetDateMinMax(IMediator mediator) : EndpointWithoutRequest<DateMinMaxDto>
{
    public override void Configure()
    {
        Get("/Analytics/Meta/DateMinMax");
        Roles("user");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var result = await mediator.Send(new GetDateMinMaxQuery(userId), ct);
        await SendOkAsync(result, ct);
    }
}
