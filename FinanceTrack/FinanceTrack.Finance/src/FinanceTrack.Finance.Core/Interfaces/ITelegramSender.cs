namespace FinanceTrack.Finance.Core.Interfaces;

public interface ITelegramSender
{
    /// <summary>
    /// Возвращает true, если бот сконфигурирован (токен задан).
    /// Используется для пропуска отправки без выброса исключения.
    /// </summary>
    bool IsConfigured { get; }

    Task SendMessageAsync(long chatId, string text, CancellationToken cancel = default);
}
