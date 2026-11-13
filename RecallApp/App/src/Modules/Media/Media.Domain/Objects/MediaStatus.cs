namespace Media.Domain.Objects;

/// <summary>
/// Состояние медиа-объекта. Переходы только вперёд (онтология Media §6.1):
/// pending -> ready | failed, ready -> deleted, failed/pending -> deleted (по таймауту).
/// </summary>
public enum MediaStatus
{
    Pending = 0,
    Ready = 1,
    Failed = 2,
    Deleted = 3,
}
