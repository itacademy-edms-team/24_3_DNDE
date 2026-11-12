namespace Sets.Domain.Sets;

/// <summary>
/// Границы постраничной выдачи списка наборов (ADR 0039).
/// </summary>
public static class SetListConstraints
{
    public const int MinPageSize = 1;
    public const int MaxPageSize = 100;
    public const int DefaultPageSize = 20;
}
