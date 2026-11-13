namespace Media.Contracts;

/// <summary>
/// Типы сообщений outbox, которые Media принимает.
/// </summary>
public static class MediaOutboxMessages
{
    /// <summary>Полная синхронизация ссылок элемента: какой файл стоит в каждом слоте.</summary>
    public const string ReferencesSync = "media.references.sync";
}
