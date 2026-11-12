using ErrorOr;

namespace Sets.Domain.Sets;

/// <summary>
/// Каталог ошибок домена Sets. Код - часть контракта для клиента (ADR 0038):
/// формат <c>&lt;Сущность&gt;.&lt;Правило&gt;</c>, по нему клиент понимает, что вывести клиенту.
/// </summary>
public static class SetErrors
{
    /// <summary>
    /// Набора нет либо он принадлежит другому пользователю:
    /// 404, чтобы не собирали карту существующих наборов засчёт возврата 403.
    /// </summary>
    public static readonly Error NotFound = Error.NotFound(
        code: "Set.NotFound",
        description: "Set does not exist or is not owned by the current user.");

    /// <summary>
    /// Личность не сопоставлена доменной строке: JIT-провижининг не выполнен (ADR 0020).
    /// </summary>
    public static readonly Error UserUnresolved = Error.Unauthorized(
        code: "Set.UserUnresolved",
        description: "Domain user is not resolved for the current request.");

    public static readonly Error TitleRequired = Error.Validation(
        code: "Set.TitleRequired",
        description: "Title must not be empty.",
        metadata: new Dictionary<string, object> { ["target"] = "set.title" });

    public static Error TitleTooLong(int length) => Error.Validation(
        code: "Set.TitleTooLong",
        description: $"Title length {length} exceeds limit {SetConstraints.TitleMaxLength}.",
        metadata: new Dictionary<string, object>
        {
            ["target"] = "set.title",
            ["max"] = SetConstraints.TitleMaxLength,
        });

    public static Error DescriptionTooLong(int length) => Error.Validation(
        code: "Set.DescriptionTooLong",
        description: $"Description length {length} exceeds limit {SetConstraints.DescriptionMaxLength}.",
        metadata: new Dictionary<string, object>
        {
            ["target"] = "set.description",
            ["max"] = SetConstraints.DescriptionMaxLength,
        });
}
