using ErrorOr;

namespace Media.Application.Storage;

/// <summary>
/// Представляет разрешение на загрузку одного файла напрямую в хранилище: подписанный URL
/// и поля формы.
/// </summary>
/// <remarks>
/// Срок и ограничения зашиты в политику, поэтому клиент не может их изменить (ADR 0037).
/// </remarks>
/// <param name="Url">Endpoint, куда уходит POST.</param>
/// <param name="Fields">Уже подписанные поля формы: policy, credential, date, signature.</param>
/// <param name="ObjectKey">Ключ, по которому сервер найдёт объект при подтверждении.</param>
/// <param name="ExpiresAt">Момент в UTC, после которого политика недействительна.</param>
public sealed record UploadPolicy(
    string Url,
    IReadOnlyDictionary<string, string> Fields,
    string ObjectKey,
    DateTime ExpiresAt);

/// <summary>
/// Представляет метаданные объекта в хранилище.
/// </summary>
/// <remarks>Нужны, чтобы проверить размер и тип файла до его чтения.</remarks>
/// <param name="SizeBytes">Фактический размер объекта.</param>
/// <param name="ContentType">Тип, с которым объект записан в хранилище.</param>
/// <param name="LastModified">Время последнего изменения объекта.</param>
public sealed record StoredObjectInfo(long SizeBytes, string? ContentType, DateTimeOffset? LastModified);

/// <summary>
/// Определяет объектное хранилище файлов модуля Media.
/// </summary>
/// <remarks>
/// Единственная точка, через которую Media получает разрешение на загрузку, проверяет, читает,
/// записывает и удаляет объекты, а также выдаёт ссылки на чтение (ADR 0008).
/// </remarks>
public interface IObjectStorage
{
    /// <summary>
    /// Создаёт политику загрузки с фиксированным ключом, заявленным типом и диапазоном размера.
    /// </summary>
    /// <param name="objectKey">Ключ, по которому объект будет записан.</param>
    /// <param name="contentType">Тип, который разрешено загрузить.</param>
    /// <param name="maxBytes">Максимальный размер загружаемого файла.</param>
    /// <param name="now">Текущий момент в UTC.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Политика загрузки либо ошибка хранилища.</returns>
    Task<ErrorOr<UploadPolicy>> CreateUploadPolicyAsync(
        string objectKey,
        string contentType,
        long maxBytes,
        DateTime now,
        CancellationToken cancellationToken);

    /// <summary>
    /// Создаёт временную ссылку на чтение.
    /// </summary>
    /// <param name="objectKey">Ключ объекта.</param>
    /// <param name="lifetime">Срок жизни ссылки.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Подписанная ссылка либо ошибка хранилища.</returns>
    Task<ErrorOr<string>> CreateReadUrlAsync(string objectKey, TimeSpan lifetime, CancellationToken cancellationToken);

    /// <summary>
    /// Возвращает метаданные объекта.
    /// </summary>
    /// <param name="objectKey">Ключ объекта.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Метаданные или <see langword="null"/>, если объекта нет.</returns>
    Task<StoredObjectInfo?> HeadAsync(string objectKey, CancellationToken cancellationToken);

    /// <summary>
    /// Читает объект целиком.
    /// </summary>
    /// <remarks>Полное чтение нужно для проверки сигнатуры и перекодирования.</remarks>
    /// <param name="objectKey">Ключ объекта.</param>
    /// <param name="maxBytes">Максимальный размер, который разрешено прочитать.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Содержимое объекта либо ошибка хранилища.</returns>
    Task<ErrorOr<byte[]>> ReadAsync(string objectKey, long maxBytes, CancellationToken cancellationToken);

    /// <summary>
    /// Записывает готовый вариант объекта.
    /// </summary>
    /// <param name="objectKey">Ключ объекта.</param>
    /// <param name="content">Содержимое файла.</param>
    /// <param name="contentType">Тип записываемого файла.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Успех либо ошибка хранилища.</returns>
    Task<ErrorOr<Success>> WriteAsync(
        string objectKey,
        byte[] content,
        string contentType,
        CancellationToken cancellationToken);

    /// <summary>
    /// Удаляет объект.
    /// </summary>
    /// <remarks>Повторное удаление не является ошибкой.</remarks>
    /// <param name="objectKey">Ключ объекта.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Успех либо ошибка хранилища.</returns>
    Task<ErrorOr<Success>> DeleteAsync(string objectKey, CancellationToken cancellationToken);
}
