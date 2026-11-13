namespace Media.Contracts;

/// <summary>Слот использования файла: место в потребителе и файл, который его занимает.</summary>
/// <param name="Reference">Ключ слота, формат задаёт потребитель (`set:{setId}:element:{elementId}:front`).</param>
/// <param name="MediaId">Файл, который занимает слот.</param>
public sealed record MediaReferenceSlot(string Reference, Guid MediaId);

/// <summary>
/// Payload сообщения <see cref="MediaOutboxMessages.ReferencesSync"/>: полный набор слотов элемента.
/// Что не входит в набор — снимается, поэтому повтор сообщения безопасен.
/// </summary>
/// <param name="OwnerId">Владелец файлов: чужие файлы привязывать нельзя.</param>
/// <param name="ElementScope">Область элемента (`set:{setId}:element:{elementId}`).</param>
/// <param name="Slots">Текущий набор слотов элемента; пустой список означает «отвязать всё».</param>
public sealed record MediaReferencesSyncPayload(
    Guid OwnerId,
    string ElementScope,
    IReadOnlyList<MediaReferenceSlot> Slots);
