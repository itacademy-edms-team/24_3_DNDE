using FinanceTrack.Finance.UseCases.Users;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.Notifications;

public class GetEmailNotificationsStatusResponse
{
    public bool IsEmailNotificationsEnabled { get; set; }
}

public class GetEmailNotificationsStatus(IMediator mediator)
    : EndpointWithoutRequest<GetEmailNotificationsStatusResponse>
{
    public override void Configure()
    {
        Get("/User/Notifications/Email/Status");
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

        var result = await mediator.Send(new GetEmailNotificationsStatusQuery(userId), ct);

        if (await this.SendResultIfNotOk(result, ct))
            return;

        await SendOkAsync(
            new GetEmailNotificationsStatusResponse { IsEmailNotificationsEnabled = result.Value },
            ct
        );
    }
}
