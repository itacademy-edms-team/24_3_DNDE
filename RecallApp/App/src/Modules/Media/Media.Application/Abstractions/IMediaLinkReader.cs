namespace Media.Application.Abstractions;

/// <summary>
/// Подписывает ссылки на готовые варианты файлов владельца.
/// </summary>
public interface IMediaLinkReader
{
    /// <summary>
    /// Возвращает ссылки по тем файлам, которые существуют, принадлежат владельцу и готовы к выдаче.
    /// Остальные в результат не попадают: вызывающий показывает заглушку. Ошибка подписи одного
    /// варианта не отменяет ответ по остальным.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, Media.Contracts.MediaFileLinks>> ReadAsync(
        Guid ownerId,
        IReadOnlyCollection<Guid> mediaIds,
        CancellationToken cancellationToken);
}
