using Media.Domain.Objects;

namespace Media.Application.Abstractions;

/// <summary>
/// Хранилище медиа-объектов: только то, что нужно сценариям модуля -
/// проверка лимита незавершённых загрузок, поиск по идентификатору и сохранение изменений.
/// </summary>
public interface IMediaObjectRepository
{
    /// <summary>Сколько незавершённых загрузок у владельца сейчас: ограничивает злоупотребление хранилищем.</summary>
    Task<int> CountPendingAsync(Guid ownerId, CancellationToken cancellationToken);

    /// <summary>Объект по идентификатору без проверки владельца: владение проверяет вызывающий сценарий.</summary>
    Task<MediaObject?> FindAsync(Guid mediaId, CancellationToken cancellationToken);

    void Add(MediaObject media);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
