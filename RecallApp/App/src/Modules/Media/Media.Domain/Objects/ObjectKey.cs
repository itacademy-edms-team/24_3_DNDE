using System;

namespace Media.Domain.Objects;

/// <summary>
/// Представляет ключ объекта в объектном хранилище.
/// </summary>
/// <remarks>
/// <para>Ключ строит домен из собственных идентификаторов: клиент не передаёт путь в хранилище,
/// а только получает адрес для отправки файла.
/// Поэтому записать объект в чужой префикс или перезаписать чужой файл, подобрав ключ, нельзя.</para>
/// <para>Структура ключа значима: по префиксу <c>tmp/</c> чистка находит брошенные загрузки,
/// а по <c>media/{ownerId}/{mediaId}/...</c> выдаются ссылки на готовые варианты.</para>
/// </remarks>
public readonly record struct ObjectKey
{
    private const int MaxLength = 512;

    private ObjectKey(string value) => Value = value;

    /// <summary>Возвращает строковое значение ключа.</summary>
    public string Value { get; }

    /// <summary>
    /// Создаёт ключ временного объекта загрузки.
    /// </summary>
    /// <param name="uploadId">Идентификатор загрузки, выданный при инициализации.</param>
    /// <returns>
    /// Ключ с префиксом <c>tmp/</c>; живёт до успешной обработки.
    /// </returns>
    public static ObjectKey Upload(Guid uploadId) => new($"tmp/{uploadId}");

    /// <summary>
    /// Создаёт ключ готового варианта.
    /// </summary>
    /// <param name="ownerId">Владелец медиа-объекта.</param>
    /// <param name="mediaId">Идентификатор медиа-объекта.</param>
    /// <param name="variant">Вид варианта.</param>
    /// <returns>
    /// Ключ готового варианта; повторный вызов с теми же аргументами даёт тот же ключ.
    /// </returns>
    public static ObjectKey Variant(Guid ownerId, Guid mediaId, MediaVariantName variant) =>
        new($"media/{ownerId}/{mediaId}/{variant.ToStorageName()}.webp");

    /// <summary>
    /// Восстанавливает ключ из значения, прочитанного из базы.
    /// </summary>
    /// <remarks>
    /// Используется при материализации из хранилища, где значение записал сам домен: пустое или
    /// слишком длинное значение означает повреждение данных, а не ошибку пользователя, и работа
    /// продолжается невозможна.
    /// </remarks>
    /// <param name="value">Строковое значение ключа.</param>
    /// <returns>Ключ.</returns>
    /// <exception cref="InvalidOperationException">Значение пустое или длиннее допустимого.</exception>
    public static ObjectKey Restore(string value) =>
        string.IsNullOrWhiteSpace(value) || value.Length > MaxLength
            ? throw new InvalidOperationException("Corrupt object key in database.")
            : new ObjectKey(value);

    public override string ToString() => Value;
}
