using Media.Contracts;

namespace Media.Application.Abstractions;

/// <summary>
/// Определяет хранилище ссылок на файлы: для каждого места, куда потребитель поставил изображение,
/// хранит, какой файл там стоит.
/// </summary>
/// <remarks>
/// <para>В основе лежит агрегат <see cref="Media.Domain.References.MediaReference"/>.</para>
/// <para>Наружу агрегаты не отдаются: ссылки меняются вместе с сохранением элемента, поэтому
/// сценариям нужна только пакетная синхронизация.</para>
/// <para>Место задаётся ключом слота, смысл которого знает только потребитель: Media хранит ключ
/// как непрозрачную строку и не разбирает его.</para>
/// </remarks>
public interface IMediaReferenceRepository
{
    /// <summary>
    /// Синхронизирует ссылки области с присланным набором.
    /// </summary>
    /// <remarks>
    /// <para>Всё, чего в наборе нет, снимается; изменившиеся слоты переводятся на новые файлы;
    /// новые добавляются. Повтор с тем же набором ничего не меняет.</para>
    /// <para>Изменения применяются в одной транзакции: область не остаётся в промежуточном
    /// состоянии.</para>
    /// <para>Владение файлами проверяет вызывающий код до вызова.</para>
    /// </remarks>
    /// <param name="elementScope">Префикс области: ссылки элемента
    /// (<c>set:{setId}:element:{elementId}</c>) или всего набора (<c>set:{setId}</c>).</param>
    /// <param name="slots">Целевое состояние области: слоты с полными ключами. Ключи вне области
    /// не поддерживаются - вызывающий присылает только слоты этой области.</param>
    /// <param name="now">Текущий момент в UTC.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <exception cref="InvalidOperationException">Слот не прошёл валидацию: пустой ключ, ключ
    /// длиннее допустимого или пустой идентификатор файла.</exception>
    Task SyncAsync(
        string elementScope,
        IReadOnlyCollection<MediaReferenceSlot> slots,
        DateTime now,
        CancellationToken cancellationToken);
}
