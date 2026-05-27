using FinanceTrack.Finance.UseCases.Users.TelegramBotNotifications;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.Notifications.TelegramBot;

public class GetTelegramBotNotificationStatusRequest
{
    public const string Route = "/User/Notifications/Telegram/Status";
}

public class GetTelegramBotNotificationStatusResponse
{
    public bool IsTelegramBotNotificationsEnabled { get; set; }
}

public class GetTelegramBotNotificationStatus(IMediator mediator)
    : EndpointWithoutRequest<GetTelegramBotNotificationStatusResponse>
{
    public override void Configure()
    {
        Get(GetTelegramBotNotificationStatusRequest.Route);
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
            new GetTelegramBotNotificationsStatusQuery(userId),
            cancel
        );

        if (await this.SendResultIfNotOk(result, cancel))
            return;

        await SendOkAsync(
            new GetTelegramBotNotificationStatusResponse
            {
                IsTelegramBotNotificationsEnabled = result.Value,
            },
            cancel
        );
    }
}
