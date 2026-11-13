using ErrorOr;

namespace Media.Domain.Objects;

/// <summary>
/// Ключ объекта в хранилище. Формируется только доменом: клиент не может выбрать путь,
/// а структура ключа нужна для чистки (`tmp/*`) и выдачи ссылок (`media/{ownerId}/{mediaId}/...`).
/// </summary>
public readonly record struct ObjectKey
{
    private const int MaxLength = 512;

    private ObjectKey(string value) => Value = value;

    public string Value { get; }

    /// <summary>Временный ключ загрузки: объект живёт до успешной обработки.</summary>
    public static ObjectKey Upload(Guid uploadId) => new($"tmp/{uploadId}");

    /// <summary>Ключ готового варианта.</summary>
    public static ObjectKey Variant(Guid ownerId, Guid mediaId, MediaVariantName variant) =>
        new($"media/{ownerId}/{mediaId}/{variant.ToStorageName()}.webp");

    /// <summary>Восстанавливает ключ из БД.</summary>
    public static ErrorOr<ObjectKey> FromStorage(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > MaxLength)
            return Error.Validation("Media.ObjectKeyInvalid", "Некорректный ключ объекта.");

        return new ObjectKey(value);
    }

    /// <summary>Восстанавливает ключ из доверенного источника (БД); нарушение формата - повреждение данных.</summary>
    public static ObjectKey Restore(string value) =>
        string.IsNullOrWhiteSpace(value) || value.Length > MaxLength
            ? throw new InvalidOperationException("Повреждённый ключ объекта в БД.")
            : new ObjectKey(value);

    public override string ToString() => Value;
}
