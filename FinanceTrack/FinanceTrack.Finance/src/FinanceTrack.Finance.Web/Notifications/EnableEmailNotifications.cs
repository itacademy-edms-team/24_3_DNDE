using FinanceTrack.Finance.UseCases.Users;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.Notifications;

public class EnableEmailNotifications(IMediator mediator) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("/User/Notifications/Email/Enable");
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

        await mediator.Send(new EnableEmailNotificationsCommand(userId), ct);
        await SendNoContentAsync(ct);
    }
}
