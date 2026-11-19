namespace Media.Contracts;

/// <summary>
/// Предоставляет типы сообщений outbox, которые принимает Media.
/// </summary>
public static class MediaOutboxMessages
{
    /// <summary>Полная синхронизация ссылок элемента: какой файл стоит в каждом слоте.</summary>
    public const string ReferencesSync = "media.references.sync";
}
