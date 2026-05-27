using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Infrastructure.Configurations;
using Telegram.Bot;

namespace FinanceTrack.Finance.Infrastructure.Notifications.Telegram;

/// <summary>
/// Отправляет сообщения через Telegram Bot API напрямую, минуя TelegramBotBase.
/// TelegramBotBase - UI-фреймворк (формы, сессии, цикл сообщений) и нужен только
/// для интерактивного общения с пользователем, при подключении уведомлений через бота например. Для исходящих уведомлений достаточно
/// одного метода Telegram.Bot SDK: SendMessage(chatId, text).
///
/// Два TelegramBotClient с одним токеном не конфликтуют: проблема возникает только при
/// нескольких polling-процессах. TelegramBotHostedService делает polling, этот клиент - только
/// отправляет (HTTP POST), возможные конфликты сведены к минимуму.
/// </summary>
public class TelegramSender : ITelegramSender
{
    private readonly ITelegramBotClient? _client;

    public TelegramSender(IOptions<TelegramBotOptions> options)
    {
        if (options.Value.IsConfigured)
            _client = new TelegramBotClient(options.Value.BotToken);
    }

    public bool IsConfigured => _client is not null;

    public async Task SendMessageAsync(long chatId, string text, CancellationToken cancel = default)
    {
        if (_client is null)
            return;

        await _client.SendMessage(chatId, text, cancellationToken: cancel);
    }
}
