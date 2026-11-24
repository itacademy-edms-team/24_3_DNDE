using ErrorOr;

namespace Media.Application.Abstractions;

/// <summary>
/// Определяет хранилище идемпотентности долгих операций модуля Media.
/// </summary>
/// <remarks>
/// <para>Долгие операции ходят во внешнее хранилище и требуют обработки изображения
/// (сейчас это подтверждение загрузки), поэтому транзакция на всё время работы не держится.</para>
/// <para>Операция по ключу выполняется ровно один раз: повтор получает сохранённый результат,
/// параллельный запрос - конфликт, а зависшая операция выполняется заново после истечения аренды
/// (онтология Media §6.6).</para>
/// </remarks>
public interface IUploadIdempotencyStore
{
    /// <summary>
    /// Выполняет операцию ровно один раз на пару (владелец, ключ).
    /// </summary>
    /// <typeparam name="TResponse">Тип сохраняемого результата.</typeparam>
    /// <param name="ownerId">Владелец из контекста запроса.</param>
    /// <param name="key">Канонизированное значение Idempotency-Key.</param>
    /// <param name="operation">Имя операции в хранилище.</param>
    /// <param name="requestHash">Хеш канонической формы запроса.</param>
    /// <param name="operationBody">Работа вне транзакции; получает токен аренды и продлевает её
    /// автоматически.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат операции: сохранённый от предыдущего вызова или полученный сейчас.</returns>
    Task<ErrorOr<TResponse>> ExecuteAsync<TResponse>(
        Guid ownerId,
        string key,
        string operation,
        string requestHash,
        Func<CancellationToken, Task<ErrorOr<TResponse>>> operationBody,
        CancellationToken cancellationToken)
        where TResponse : class;
}
