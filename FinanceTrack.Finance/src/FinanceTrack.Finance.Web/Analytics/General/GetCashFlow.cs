using FinanceTrack.Finance.UseCases.Analytics.Dto;
using FinanceTrack.Finance.UseCases.Analytics.General;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.Analytics.General;

public class GetCashFlowRequest
{
    public const string Route = "/Analytics/CashFlow";

    [QueryParam]
    public DateOnly From { get; set; }

    [QueryParam]
    public DateOnly To { get; set; }
}

public class GetCashFlow(IMediator mediator) : Endpoint<GetCashFlowRequest, CashFlowDto>
{
    public override void Configure()
    {
        Get(GetCashFlowRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(GetCashFlowRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var result = await mediator.Send(new GetGeneralCashFlowQuery(userId, req.From, req.To), ct);
        await SendOkAsync(result, ct);
    }
}
