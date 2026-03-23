using FinanceTrack.Finance.UseCases.Analytics;
using FinanceTrack.Finance.UseCases.Analytics.Dto;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.Analytics.General;

public class GetYearMinMax(IMediator mediator) : EndpointWithoutRequest<YearMinMaxDto>
{
    public override void Configure()
    {
        Get("/Analytics/Meta/YearMinMax");
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

        var result = await mediator.Send(new GetYearMinMaxQuery(userId), ct);

        if (await this.SendResultIfNotOk(result, ct))
            return;

        await SendOkAsync(result.Value, ct);
    }
}
