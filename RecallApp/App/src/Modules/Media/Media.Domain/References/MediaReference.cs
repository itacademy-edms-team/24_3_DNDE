using ErrorOr;
using Media.Domain.Objects;

namespace Media.Domain.References;

/// <summary>
/// Представляет ссылку потребителя на медиа-объект.
/// </summary>
/// <remarks>
/// <para>Отдельный корень агрегата (онтология Media §4.2):
/// ссылки меняются пачкой при каждом сохранении элемента,
/// поэтому держать их внутри агрегата файла нельзя.</para>
/// <para><c>Reference</c> - непрозрачный для Media
/// ключ слота (например <c>set:{setId}:element:{elementId}:front</c>):
/// смысл строки знает только потребитель, Media использует её как уникальный идентификатор места.</para>
/// </remarks>
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

    /// <summary>
    /// Возвращает идентификатор ссылки.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Возвращает файл, на который указывает слот.
    /// </summary>
    public Guid MediaId { get; private set; }

    /// <summary>
    /// Возвращает ключ слота, заданный потребителем.
    /// </summary>
    public string Reference { get; private set; } = null!;

    /// <summary>
    /// Возвращает момент создания ссылки в UTC.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Возвращает момент последней привязки слота к другому файлу в UTC.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Создаёт ссылку на конкретный слот.
    /// </summary>
    /// <param name="mediaId">Файл, на который указывает слот.</param>
    /// <param name="reference">Ключ слота; обрезается по краям.</param>
    /// <param name="now">Текущий момент в UTC.</param>
    /// <returns>
    /// Новая ссылка либо ошибка:
    /// <see cref="MediaErrors.NotFound"/> (пустой файл)
    /// или <see cref="MediaErrors.ReferenceInvalid"/> (пустой или слишком длинный ключ).
    /// </returns>
    public static ErrorOr<MediaReference> Create(Guid mediaId, string? reference, DateTime now)
    {
        if (mediaId == Guid.Empty)
            return MediaErrors.NotFound;

        if (string.IsNullOrWhiteSpace(reference) || reference.Length > MaxReferenceLength)
            return MediaErrors.ReferenceInvalid;

        return new MediaReference(Guid.NewGuid(), mediaId, reference.Trim(), now);
    }

    /// <summary>
    /// Переводит слот на другой файл.
    /// </summary>
    /// <remarks>
    /// Замена картинки перезаписывает слот, а не создаёт новую ссылку.
    /// Повторная привязка к тому же файлу ничего не меняет, включая <see cref="UpdatedAt"/>.
    /// </remarks>
    /// <param name="mediaId">Новый файл слота.</param>
    /// <param name="now">Текущий момент в UTC.</param>
    /// <returns>
    /// Успех либо ошибка <see cref="MediaErrors.NotFound"/> (пустой файл).
    /// </returns>
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

    /// <summary>
    /// Проверяет, что ссылка относится к области элемента или набора.
    /// </summary>
    /// <param name="scope">Область: ключ элемента или набора.</param>
    /// <returns><see langword="true"/>, если слот входит в область.</returns>
    public bool BelongsTo(string scope) => IsInScope(Reference, scope);

    /// <summary>
    /// Проверяет, принадлежит ли ключ слота области.
    /// </summary>
    /// <remarks>
    /// Совпадением считается либо точное равенство, либо префикс <c>scope:</c>.
    /// Сравнение порядковое: регистр значим.
    /// </remarks>
    /// <param name="reference">Ключ слота.</param>
    /// <param name="scope">Область: ключ элемента или набора.</param>
    /// <returns><see langword="true"/>, если ключ совпадает с областью или продолжает её двоеточием;
    /// <see langword="false"/> для <see langword="null"/> и любых других значений.</returns>
    public static bool IsInScope(string? reference, string scope) =>
        reference is not null
        && (reference.Equals(scope, StringComparison.Ordinal)
            || reference.StartsWith(scope + ":", StringComparison.Ordinal));
}
