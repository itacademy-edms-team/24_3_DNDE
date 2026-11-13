using ErrorOr;

namespace Media.Contracts;

/// <summary>
/// Проверка «файл готов и принадлежит автору» перед сохранением чужой сущности.
/// Вызывается синхронно: иначе плохая ссылка успеет попасть в БД, а ошибка придёт позже
/// (онтология Media §7.4, ADR 0040).
/// </summary>
public interface IMediaReadiness
{
    /// <summary>
    /// Проверяет, что все файлы существуют, находятся в состоянии `ready` и принадлежат владельцу.
    /// </summary>
    /// <param name="ownerId">Автор сохраняемой сущности</param>
    /// <param name="mediaIds">Идентификаторы файлов из сохраняемых данных.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task<ErrorOr<Success>> EnsureReadyAsync(
        Guid ownerId,
        IReadOnlyCollection<Guid> mediaIds,
        CancellationToken cancellationToken);
}

/// <summary>
/// Ссылки на файлы для отдачи клиенту: подписываются в момент запроса, поэтому не хранятся в данных.
/// </summary>
public interface IMediaLinksQuery
{
    /// <summary>
    /// Возвращает ссылки по набору файлов; отсутствующие, чужие и неготовые в результат не попадают.
    /// </summary>
    /// <param name="ownerId">Владелец, от имени которого читаются файлы.</param>
    /// <param name="mediaIds">Идентификаторы файлов из прочитанных данных.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task<IReadOnlyDictionary<Guid, MediaFileLinks>> GetLinksAsync(
        Guid ownerId,
        IReadOnlyCollection<Guid> mediaIds,
        CancellationToken cancellationToken);
}
