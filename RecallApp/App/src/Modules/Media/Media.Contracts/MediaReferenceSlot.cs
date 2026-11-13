namespace Media.Contracts;

/// <summary>Слот использования файла: место в потребителе и файл, который его занимает.</summary>
/// <param name="Reference">Ключ слота, формат задаёт потребитель (`set:{setId}:element:{elementId}:front`).</param>
/// <param name="MediaId">Файл, который занимает слот.</param>
public sealed record MediaReferenceSlot(string Reference, Guid MediaId);
