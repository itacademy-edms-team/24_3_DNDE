using Media.Domain.Objects;

namespace Media.Application.Abstractions;

/// <summary>
/// Хранилище медиа-объектов: только то, что нужно сценариям модуля -
/// проверка лимита незавершённых загрузок, поиск по идентификатору, учёт ссылок и сохранение изменений.
/// </summary>
public interface IMediaObjectRepository
{
    /// <summary>
    /// Считает, сколько незавершённых загрузок у владельца сейчас: учитываются только те, чьё окно загрузки ещё не истекло.
    /// </summary>
    Task<int> CountPendingAsync(Guid ownerId, DateTime now, CancellationToken cancellationToken);

    /// <summary>
    /// Переводит в `failed` те загрузки, чьё окно истекло: слот освобождается, а строка честно говорит,
    /// что файл не был загружен. Возвращает число затронутых строк.
    /// </summary>
    Task<int> MarkExpiredUploadsAsync(Guid ownerId, DateTime now, CancellationToken cancellationToken);

    /// <summary>Объект по идентификатору без проверки владельца: владение проверяет вызывающий сценарий.</summary>
    Task<MediaObject?> FindAsync(Guid mediaId, CancellationToken cancellationToken);

    /// <summary>Сколько элементов ссылается на файл: ненулевое значение запрещает удаление.</summary>
    Task<int> CountReferencesAsync(Guid mediaId, CancellationToken cancellationToken);

    /// <summary>
    /// Выполняет действие в транзакции под блокировкой строки файла: удаление не должно гоняться
    /// с синхронизацией ссылок. Ошибка откатывает транзакцию.
    /// </summary>
    Task<TResult> ExecuteDeletionAsync<TResult>(
        MediaObject media,
        Func<CancellationToken, Task<TResult>> action,
        CancellationToken cancellationToken);

    void Add(MediaObject media);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
