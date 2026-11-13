using Media.Contracts;

namespace Media.Application.Abstractions;

/// <summary>
/// Журнал использования файлов: для каждого места, куда потребитель поставил изображение
/// (например, лицевая сторона карточки), хранит, какой файл там стоит.
/// Приводит набор таких мест области к присланному состоянию - снимает лишние,
/// перезаписывает изменённые, добавляет новые.
/// </summary>
public interface IMediaReferenceRepository
{
    /// <summary>
    /// Приводит ссылки области к присланному набору: всё, чего в наборе нет, снимается,
    /// изменившиеся слоты переводятся на новые файлы.
    /// Повтор с тем же набором ничего не меняет.
    /// </summary>
    Task SyncAsync(
        Guid ownerId,
        string elementScope,
        IReadOnlyCollection<MediaReferenceSlot> slots,
        DateTime now,
        CancellationToken cancellationToken);
}
