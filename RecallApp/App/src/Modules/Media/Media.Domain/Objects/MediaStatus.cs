namespace Media.Domain.Objects;

/// <summary>
/// Указывает состояние жизненного цикла медиа-объекта.
/// </summary>
/// <remarks>
/// <para>Переходы только вперёд:
/// <c>pending</c> -> <c>ready</c> | <c>failed</c>, далее <c>deleted</c>;
/// <c>failed</c> и <c>pending</c> попадают в <c>deleted</c> по истечении окна (онтология Media §6.1).</para>
/// <para>Числовые значения сохраняются в базе как есть: менять их нельзя,
/// добавлять новые состояния можно только в конец.</para>
/// </remarks>
public enum MediaStatus
{
    Pending = 0,
    Ready = 1,
    Failed = 2,
    Deleted = 3,
}
