using FinanceTrack.Finance.UseCases.Users.TelegramBotNotifications;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.Notifications.TelegramBot;

public class DisableTelegramBotNotificationsRequest
{
    public const string Route = "/User/Notifications/Telegram/Disable";
}

public class DisableTelegramBotNotifications(IMediator mediator) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post(DisableTelegramBotNotificationsRequest.Route);
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

        var result = await mediator.Send(
            new DisableTelegramBotNotificationsCommand(userId),
            cancel
        );

        if (await this.SendResultIfNotOk(result, cancel))
            return;

        await SendOkAsync(cancel);
    }
}
