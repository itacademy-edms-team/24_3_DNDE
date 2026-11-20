using ErrorOr;

namespace Media.Contracts;

/// <summary>
/// Определяет проверку готовности файлов, на которые ссылается сохраняемый элемент набора.
/// </summary>
/// <remarks>
/// <para>Контракт для Sets: перед сохранением элемента проверяется, что все его изображения
/// существуют, находятся в состоянии <c>ready</c> и принадлежат владельцу.</para>
/// <para>Проверка синхронная: иначе невалидная ссылка успела бы попасть в БД, а ошибка пришла бы
/// позже через outbox (онтология Media §7.4, ADR 0040).</para>
/// </remarks>
public interface IMediaReadiness
{
    /// <summary>
    /// Проверяет, что все файлы существуют, готовы и принадлежат владельцу.
    /// </summary>
    /// <param name="ownerId">Владелец файлов и сохраняемого элемента.</param>
    /// <param name="mediaIds">Идентификаторы файлов из сохраняемых данных;
    /// пустой набор - успех без обращения к БД.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Успех либо ошибка с кодом <c>Media.NotFound</c> (файла нет или он чужой) или
    /// <c>Media.NotReady</c> (файл не в состоянии <c>ready</c>).</returns>
    Task<ErrorOr<Success>> EnsureReadyAsync(
        Guid ownerId,
        IReadOnlyCollection<Guid> mediaIds,
        CancellationToken cancellationToken);
}

/// <summary>
/// Определяет чтение подписанных ссылок на файлы.
/// </summary>
/// <remarks>
/// Ссылки имеют ограниченный срок жизни и подписываются на каждый вызов,
/// поэтому хранится только идентификатор файла, а свежий адрес запрашивается заново.
/// </remarks>
public interface IMediaLinksQuery
{
    /// <summary>
    /// Возвращает подписанные ссылки по набору файлов.
    /// </summary>
    /// <param name="ownerId">Владелец, от имени которого читаются файлы.</param>
    /// <param name="mediaIds">Идентификаторы файлов из прочитанных данных.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Ссылки по доступным файлам;
    /// отсутствующие, чужие и неготовые в результат не попадают.</returns>
    Task<IReadOnlyDictionary<Guid, MediaFileLinks>> GetLinksAsync(
        Guid ownerId,
        IReadOnlyCollection<Guid> mediaIds,
        CancellationToken cancellationToken);
}
