using FinanceTrack.Finance.UseCases.Users;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.Notifications;

public class DisableEmailNotifications(IMediator mediator) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("/User/Notifications/Email/Disable");
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

        await mediator.Send(new DisableEmailNotificationsCommand(userId), ct);
        await SendNoContentAsync(ct);
    }
}
