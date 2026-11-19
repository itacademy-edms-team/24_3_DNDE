using Media.Contracts;

namespace Media.Application.Abstractions;

/// <summary>
/// Определяет чтение подписанных ссылок на готовые варианты файлов.
/// </summary>
/// <remarks>
/// Результат может быть неполным: отсутствие ключа означает, что файл недоступен для выдачи,
/// и ошибкой не является.
/// </remarks>
public interface IMediaLinkReader
{
	/// <summary>
	/// Читает подписанные ссылки на готовые варианты файлов.
	/// </summary>
	/// <param name="ownerId">Владелец файлов.</param>
	/// <param name="mediaIds">Идентификаторы файлов.</param>
	/// <param name="cancellationToken">Токен отмены.</param>
	/// <returns>Ссылки для доступных файлов владельца; для остальных идентификаторов ключа нет.</returns>
    Task<IReadOnlyDictionary<Guid, MediaFileLinks>> ReadAsync(
        Guid ownerId,
        IReadOnlyCollection<Guid> mediaIds,
        CancellationToken cancellationToken);
}
