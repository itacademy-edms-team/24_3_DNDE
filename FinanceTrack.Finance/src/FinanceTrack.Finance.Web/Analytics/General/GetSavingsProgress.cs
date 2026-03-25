using FinanceTrack.Finance.UseCases.Analytics.Dto;
using FinanceTrack.Finance.UseCases.Analytics.General;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.Analytics.General;

public class GetSavingsProgress(IMediator mediator) : EndpointWithoutRequest<SavingsProgressDto>
{
    public override void Configure()
    {
        Get("/Analytics/Savings");
        Roles("user");
    }

    public override async Task HandleAsync(CancellationToken cancel)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(cancel);
            return;
        }

        var result = await mediator.Send(new GetGeneralSavingsProgressQuery(userId), cancel);
        await SendOkAsync(result, cancel);
    }
}
