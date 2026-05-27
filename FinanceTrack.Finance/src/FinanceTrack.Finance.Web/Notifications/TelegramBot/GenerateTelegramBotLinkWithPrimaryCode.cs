using FinanceTrack.Finance.UseCases.Users.TelegramBotNotifications;
using FinanceTrack.Finance.Web.Extensions;

namespace FinanceTrack.Finance.Web.Notifications.TelegramBot;

public class GenerateTelegramBotLinkWithPrimaryCodeRequest
{
    public const string Route = "/User/Notifications/Telegram/LinkWithPrimaryCode";
}

public class GenerateTelegramBotLinkWithPrimaryCodeResponse
{
    public string TelegramBotLinkWithCode { get; set; } = default!;
}

public class GenerateTelegramBotLinkWithPrimaryCode(IMediator mediator)
    : EndpointWithoutRequest<GenerateTelegramBotLinkWithPrimaryCodeResponse>
{
    public override void Configure()
    {
        Post(GenerateTelegramBotLinkWithPrimaryCodeRequest.Route);
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

        var result = await mediator.Send(new GenerateLinkWithPrimaryCodeCommand(userId), cancel);

        if (await this.SendResultIfNotOk(result, cancel))
            return;

        await SendOkAsync(
            new GenerateTelegramBotLinkWithPrimaryCodeResponse
            {
                TelegramBotLinkWithCode = result.Value,
            },
            cancel
        );
    }
}
