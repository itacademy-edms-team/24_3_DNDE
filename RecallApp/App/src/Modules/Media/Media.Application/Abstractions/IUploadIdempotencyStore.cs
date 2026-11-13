using ErrorOr;

namespace Media.Application.Abstractions;

/// <summary>
/// Журнал идемпотентности долгих операций модуля Media - тех, что ходят во внешнее хранилище
/// и требуют обработки изображения (сейчас это подтверждение загрузки).
/// Гарантирует, что операция по ключу выполнится ровно один раз:
/// повтор получает сохранённый результат,
/// параллельный запрос - конфликт,
/// а зависшая операция выполняется заново после истечения аренды (онтология Media §6.6).
/// </summary>
public interface IUploadIdempotencyStore
{
    /// <summary>
    /// Выполняет операцию ровно один раз на пару (владелец, ключ).
    /// Повтор получает сохранённый результат, параллельный запрос с живой арендой - 409,
    /// а после истечения аренды следующий запрос с тем же ключом выполняет операцию заново.
    /// </summary>
    /// <param name="ownerId">Владелец из контекста запроса.</param>
    /// <param name="key">Канонизированное значение Idempotency-Key.</param>
    /// <param name="operation">Имя операции в журнале.</param>
    /// <param name="requestHash">Хеш канонической формы запроса.</param>
    /// <param name="operationBody">Работа вне транзакции; получает токен аренды и продлевает её автоматически.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task<ErrorOr<TResponse>> ExecuteAsync<TResponse>(
        Guid ownerId,
        string key,
        string operation,
        string requestHash,
        Func<CancellationToken, Task<ErrorOr<TResponse>>> operationBody,
        CancellationToken cancellationToken)
        where TResponse : class;
}
