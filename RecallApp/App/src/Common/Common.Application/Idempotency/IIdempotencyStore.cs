using ErrorOr;

namespace Common.Application.Idempotency;

/// <summary>
/// Определяет хранилище ключей идемпотентности модуля.
/// </summary>
/// <remarks>
/// <para>Резервирование ключа и бизнес-операция выполняются в одной транзакции, поэтому
/// состояния «в процессе» для этого режима не существует.</para>
/// <para>Режим рассчитан на короткие операции, целиком укладывающиеся в транзакцию БД.
/// Для операций с сетевым вводом-выводом (загрузка и обработка медиа, внешние API) нужен другой
/// режим: короткий коммит ключа со статусом «в процессе» и арендой, работа вне транзакции,
/// финализация и восстановление зависших ключей. Пока таких операций нет, реализация одна.</para>
/// </remarks>
public interface IIdempotencyStore
{
    /// <summary>
    /// Выполняет операцию ровно один раз на пару (владелец, ключ).
    /// Повтор с тем же телом возвращает сохранённый ответ, с другим телом - 409.
    /// </summary>
    /// <param name="ownerId">Владелец из контекста запроса: ключи не пересекаются между пользователями.</param>
    /// <param name="key">Значение заголовка Idempotency-Key.</param>
    /// <param name="operation">Имя операции: один ключ не должен переиспользоваться разными операциями.</param>
    /// <param name="requestHash">Хеш канонической формы запроса.</param>
    /// <param name="operationBody">Бизнес-операция; её изменения коммитятся вместе с ключом.</param>
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
