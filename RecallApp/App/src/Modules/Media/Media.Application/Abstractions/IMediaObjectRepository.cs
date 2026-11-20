using Media.Domain.Objects;

namespace Media.Application.Abstractions;

/// <summary>
/// Определяет хранилище медиа-объектов.
/// </summary>
/// <remarks>
/// Контракт намеренно узкий:
/// операции добавляются под потребности сценариев модуля, а не под общий набор выборок.
/// </remarks>
public interface IMediaObjectRepository
{
    /// <summary>
    /// Считает незавершённые загрузки владельца.
    /// </summary>
    /// <remarks>
    /// Учитываются только загрузки с действующим окном: просроченные лимит не занимают.
    /// </remarks>
    /// <param name="ownerId">Владелец загрузок.</param>
    /// <param name="now">Текущий момент в UTC.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Число загрузок в состоянии <c>pending</c>, срок которых ещё не истёк.</returns>
    Task<int> CountPendingAsync(Guid ownerId, DateTime now, CancellationToken cancellationToken);

    /// <summary>
    /// Переводит в <c>failed</c> загрузки с истёкшим сроком.
    /// </summary>
    /// <remarks>
    /// Вызывается до проверки лимита: иначе просроченные загрузки продолжают занимать места.
    /// </remarks>
    /// <param name="ownerId">Владелец загрузок.</param>
    /// <param name="now">Текущий момент в UTC.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Число затронутых строк.</returns>
    Task<int> MarkExpiredUploadsAsync(Guid ownerId, DateTime now, CancellationToken cancellationToken);

    /// <summary>
    /// Находит объект по идентификатору.
    /// </summary>
    /// <remarks>Владение не проверяется: это делает вызывающий сценарий.</remarks>
    /// <param name="mediaId">Идентификатор объекта.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Объект или <see langword="null"/>, если его нет.</returns>
    Task<MediaObject?> FindAsync(Guid mediaId, CancellationToken cancellationToken);

    /// <summary>
    /// Считает ссылки на файл.
    /// </summary>
    /// <param name="mediaId">Идентификатор объекта.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Число ссылок; ненулевое значение запрещает удаление.</returns>
    Task<int> CountReferencesAsync(Guid mediaId, CancellationToken cancellationToken);

    /// <summary>
    /// Выполняет действие в транзакции под блокировкой строки файла.
    /// </summary>
    /// <remarks>
    /// Блокировка нужна, чтобы удаление не гонялось с синхронизацией ссылок:
    /// иначе файл может стать используемым уже после проверки.
    /// Ошибка действия откатывает транзакцию.
    /// </remarks>
    /// <typeparam name="TResult">Тип результата действия.</typeparam>
    /// <param name="media">Объект, строку которого нужно заблокировать.</param>
    /// <param name="action">Действие внутри транзакции.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат действия.</returns>
    Task<TResult> ExecuteDeletionAsync<TResult>(
        MediaObject media,
        Func<CancellationToken, Task<TResult>> action,
        CancellationToken cancellationToken);

    /// <summary>
    /// Регистрирует новый объект для сохранения.
    /// </summary>
    /// <param name="media">Новый объект.</param>
    void Add(MediaObject media);

    /// <summary>
    /// Сохраняет изменения.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
