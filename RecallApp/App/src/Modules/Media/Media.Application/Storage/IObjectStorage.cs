using ErrorOr;

namespace Media.Application.Storage;

/// <summary>
/// Разрешение на загрузку одного файла напрямую в хранилище: подписанный URL и поля формы.
/// Срок и ограничения зашиты в политику, клиент их не меняет (ADR 0037).
/// </summary>
/// <param name="Url">Endpoint, куда уходит POST.</param>
/// <param name="Fields">Уже подписанные поля формы: policy, credential, date, signature.</param>
/// <param name="ObjectKey">Ключ, по которому сервер найдёт объект при подтверждении.</param>
/// <param name="ExpiresAt">Момент, после которого политика недействительна.</param>
public sealed record UploadPolicy(
    string Url,
    IReadOnlyDictionary<string, string> Fields,
    string ObjectKey,
    DateTime ExpiresAt);

/// <summary>Метаданные загруженного объекта: нужны для проверки размера и типа после загрузки.</summary>
/// <param name="SizeBytes">Фактический размер объекта.</param>
/// <param name="ContentType">Тип, с которым объект записан в хранилище.</param>
/// <param name="LastModified">Время последнего изменения объекта.</param>
public sealed record StoredObjectInfo(long SizeBytes, string? ContentType, DateTimeOffset? LastModified);

/// <summary>
/// Хранилище файлов модуля: единственная точка, через которую Media получает разрешение на загрузку,
/// проверяет объекты, читает, записывает, удаляет их и выдаёт ссылки на чтение (ADR 0008).
/// </summary>
public interface IObjectStorage
{
    /// <summary>
    /// Создаёт политику загрузки: фиксированный ключ, заявленный тип и диапазон размера.
    /// </summary>
    Task<ErrorOr<UploadPolicy>> CreateUploadPolicyAsync(
        string objectKey,
        string contentType,
        long maxBytes,
        DateTime now,
        CancellationToken cancellationToken);

    /// <summary>Создаёт временную ссылку на чтение с ограниченным сроком жизни.</summary>
    Task<ErrorOr<string>> CreateReadUrlAsync(string objectKey, TimeSpan lifetime, CancellationToken cancellationToken);
    
    /// <summary>Возвращает метаданные объекта или null, если объекта нет.</summary>
    Task<StoredObjectInfo?> HeadAsync(string objectKey, CancellationToken cancellationToken);
    
    /// <summary>Читает объект целиком: нужно для проверки сигнатуры и перекодирования.</summary>
    Task<ErrorOr<byte[]>> ReadAsync(string objectKey, long maxBytes, CancellationToken cancellationToken);

    /// <summary>Записывает готовый вариант объекта обратно в хранилище.</summary>
    Task<ErrorOr<Success>> WriteAsync(
        string objectKey,
        byte[] content,
        string contentType,
        CancellationToken cancellationToken);

    /// <summary>Удаляет объект. Повторное удаление не является ошибкой.</summary>
    Task<ErrorOr<Success>> DeleteAsync(string objectKey, CancellationToken cancellationToken);
}
