using FinanceTrack.Finance.UseCases.Analytics;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.Analytics;

public class GetSavingsProgress(IMediator mediator) : EndpointWithoutRequest<SavingsProgressDto>
{
    public override void Configure()
    {
        Get("/Analytics/Savings");
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

        var result = await mediator.Send(new GetSavingsProgressQuery(userId), ct);
        await SendOkAsync(result, ct);
    }
}
