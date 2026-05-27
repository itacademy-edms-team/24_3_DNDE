using Ardalis.Result;
using FinanceTrack.Finance.UseCases.Users.TelegramBotNotifications;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace FinanceTrack.Finance.Infrastructure.Notifications.Telegram;

/// <summary>
/// The start form for the Telegram bot.
/// Uses IServiceScopeFactory to create a DI scope per operation, because TelegramBotBase
/// instantiates forms from the root IServiceProvider - resolving scoped services (IRepository,
/// IUnitOfWork) directly from root would throw InvalidOperationException.
/// </summary>
public class StartForm(IServiceScopeFactory scopeFactory) : AutoCleanForm
{
    private string? _message;
    private ButtonForm? _buttons;

    public override async Task Load(MessageResult message)
    {
        var text = message.MessageText?.Trim() ?? string.Empty;

        if (text.StartsWith("/start "))
        {
            await HandlePrimaryCode(text["/start ".Length..]);
            return;
        }

        // Команда /disconnect — сразу отключаем без промежуточного экрана с кнопкой.
        // Удобно когда инлайн-кнопки уже не видны (например, после напоминания).
        if (message.IsBotCommand && message.BotCommand == "/disconnect")
        {
            await HandleDisconnect();
            return;
        }

        // /start без параметров или любое другое сообщение — приветствие с кнопкой
        _buttons = new ButtonForm();
        _buttons.AddButtonRow(
            new ButtonBase(
                "🔕 Отключить уведомления",
                new CallbackData("n", "disconnect").Serialize()
            )
        );
        _message =
            "👋 Добро пожаловать в бот FinanceTrack!\n\n"
            + "Для подключения уведомлений — перейдите по ссылке с сайта.\n"
            + "Для отключения — нажмите кнопку ниже или введите /disconnect.";
    }

    public override async Task Action(MessageResult message)
    {
        var call = message.GetData<CallbackData>();
        await message.ConfirmAction();

        if (call?.Value != "disconnect")
            return;

        await HandleDisconnect();
    }

    public override async Task Render(MessageResult message)
    {
        if (_message is null)
            return;

        await Device.Send(_message, _buttons);

        _message = null;
        _buttons = null;
    }

    private async Task HandleDisconnect()
    {
        using var scope = scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var result = await mediator.Send(
            new DisableTelegramBotNotificationsByChatIdCommand(Device.DeviceId)
        );

        if (result.IsSuccess)
        {
            _message = "✅ Telegram-уведомления успешно отключены.";
            _buttons = null; // не показываем кнопку — уже отключили
        }
        else
        {
            _message = $"❌ {string.Join(", ", result.Errors)}";
            // _buttons остаются от Load() — кнопка показывается снова
        }
    }

    private async Task HandlePrimaryCode(string param)
    {
        if (!Guid.TryParseExact(param, "N", out var primaryCode))
        {
            _message = "❌ Ссылка недействительна. Запросите новую ссылку на сайте.";
            return;
        }

        using var scope = scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var result = await mediator.Send(
            new ProcessTelegramPrimaryCodeCommand(primaryCode, Device.DeviceId)
        );

        if (result.IsSuccess)
        {
            _message =
                $"✅ Ваш код подтверждения: `{result.Value}`\n\n"
                + "Введите его на сайте для завершения подключения уведомлений.";
            return;
        }

        _message = result.Status switch
        {
            ResultStatus.NotFound =>
                "❌ Ссылка недействительна или устарела. Запросите новую ссылку на сайте.",
            _ => $"❌ {string.Join(", ", result.Errors)}",
        };
    }
}
