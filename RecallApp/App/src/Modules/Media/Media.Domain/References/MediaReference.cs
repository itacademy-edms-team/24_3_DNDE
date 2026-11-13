using ErrorOr;
using Media.Domain.Objects;

namespace Media.Domain.References;

/// <summary>
/// Ссылка потребителя на медиа-объект. Отдельный корень агрегата (онтология Media §4.2):
/// ссылки меняются пачкой при каждом сохранении элемента, тащить их в агрегат файла нельзя.
/// <para>
/// `Reference` - непрозрачный для Media ключ слота (`set:{setId}:element:{elementId}:front`):
/// смысл строки знает только потребитель, Media использует её как уникальный идентификатор места.
/// </para>
/// </summary>
public sealed class MediaReference
{
    private const int MaxReferenceLength = 200;

    private MediaReference() { }

    private MediaReference(Guid id, Guid mediaId, string reference, DateTime now)
    {
        Id = id;
        MediaId = mediaId;
        Reference = reference;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public Guid Id { get; private set; }

    public Guid MediaId { get; private set; }

    public string Reference { get; private set; } = null!;

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    /// <summary>Создаёт ссылку на конкретный слот.</summary>
    public static ErrorOr<MediaReference> Create(Guid mediaId, string? reference, DateTime now)
    {
        if (mediaId == Guid.Empty)
            return MediaErrors.NotFound;

        if (string.IsNullOrWhiteSpace(reference) || reference.Length > MaxReferenceLength)
            return Error.Validation("Media.ReferenceInvalid", "Некорректный ключ ссылки.");

        return new MediaReference(Guid.NewGuid(), mediaId, reference.Trim(), now);
    }

    /// <summary>Переводит слот на другой файл: замена картинки перезаписывает слот, а не создаёт новый.</summary>
    public ErrorOr<Success> PointTo(Guid mediaId, DateTime now)
    {
        if (mediaId == Guid.Empty)
            return MediaErrors.NotFound;

        if (MediaId == mediaId)
            return Result.Success;

        MediaId = mediaId;
        UpdatedAt = now;
        return Result.Success;
    }

    /// <summary>Проверяет, что ссылка относится к области элемента или набора.</summary>
    public bool BelongsTo(string scope) =>
        Reference.Equals(scope, StringComparison.Ordinal)
        || Reference.StartsWith(scope + ":", StringComparison.Ordinal);
}
