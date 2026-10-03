using FinanceTrack.Finance.UseCases.Users.TelegramBotNotifications;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.Notifications.TelegramBot;

public class EnableTelegramBotNotificationsRequest
{
    public const string Route = "/User/Notifications/Telegram/Enable";
    public int ConfirmationCode { get; set; }
}

public class EnableTelegramBotNotifications(IMediator mediator)
    : Endpoint<EnableTelegramBotNotificationsRequest>
{
    public override void Configure()
    {
        Post(EnableTelegramBotNotificationsRequest.Route);
        Roles("user");
    }

    public override async Task HandleAsync(
        EnableTelegramBotNotificationsRequest req,
        CancellationToken cancel
    )
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await SendUnauthorizedAsync(cancel);
            return;
        }

        var result = await mediator.Send(
            new EnableTelegramBotNotificationsCommand(userId, req.ConfirmationCode),
            cancel
        );

        if (await this.SendResultIfNotOk(result, cancel))
            return;

        await SendOkAsync(cancel);
    }
}
