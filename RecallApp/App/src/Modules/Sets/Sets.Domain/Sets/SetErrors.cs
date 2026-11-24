using ErrorOr;

namespace Sets.Domain.Sets;

/// <summary>
/// Предоставляет каталог ошибок агрегата набора.
/// </summary>
/// <remarks>
/// Код ошибки является частью контракта для клиента (ADR 0038)
/// и имеет формат <c>&lt;Сущность&gt;.&lt;Правило&gt;</c>: по нему выбирается текст для пользователя.
/// Описание ошибки предназначено для логов и в интерфейс не попадает.
/// Метаданные используются в логах и внутренних обработчиках;
/// в ответ ProblemDetails сейчас уходит только <c>target</c>.
/// </remarks>
public static class SetErrors
{
    /// <summary>
    /// Набора нет либо он принадлежит другому пользователю.
    /// </summary>
    public static readonly Error NotFound = Error.NotFound(
        code: "Set.NotFound",
        description: "Set does not exist or is not owned by the current user.");

    /// <summary>
    /// Личность не сопоставлена доменной строке.
    /// </summary>
    /// <remarks>JIT-провижининг не выполнен (ADR 0020).</remarks>
    public static readonly Error UserUnresolved = Error.Unauthorized(
        code: "Set.UserUnresolved",
        description: "Domain user is not resolved for the current request.");

    /// <summary>
    /// Название набора не задано или состоит из пробелов.
    /// </summary>
    public static readonly Error TitleRequired = Error.Validation(
        code: "Set.TitleRequired",
        description: "Title must not be empty.",
        metadata: new Dictionary<string, object> { ["target"] = "set.title" });

    /// <summary>
    /// Создаёт ошибку превышения максимальной допустимой длины названия набора.
    /// </summary>
    /// <param name="length">Фактическая длина после нормализации.</param>
    /// <returns>
    /// Ошибка валидации с кодом <c>Set.TitleTooLong</c>
    /// и фактической и максимальной длиной в метаданных.
    /// </returns>
    public static Error TitleTooLong(int length) => Error.Validation(
        code: "Set.TitleTooLong",
        description: $"Title length {length} exceeds limit {SetConstraints.TitleMaxLength}.",
        metadata: new Dictionary<string, object>
        {
            ["target"] = "set.title",
            ["length"] = length,
            ["max"] = SetConstraints.TitleMaxLength,
        });

    /// <summary>
    /// Создаёт ошибку превышения максимальной допустимой длины для описания набора.
    /// </summary>
    /// <param name="length">Фактическая длина после нормализации.</param>
    /// <returns>
    /// Ошибка валидации с кодом <c>Set.DescriptionTooLong</c>
    /// и фактической и максимальной длиной в метаданных.
    /// </returns>
    public static Error DescriptionTooLong(int length) => Error.Validation(
        code: "Set.DescriptionTooLong",
        description: $"Description length {length} exceeds limit {SetConstraints.DescriptionMaxLength}.",
        metadata: new Dictionary<string, object>
        {
            ["target"] = "set.description",
            ["length"] = length,
            ["max"] = SetConstraints.DescriptionMaxLength,
        });

    /// <summary>
    /// Курсор постраничной выдачи наборов не разбирается.
    /// </summary>
    public static readonly Error CursorInvalid = Error.Validation(
        code: "Set.CursorInvalid",
        description: "Cursor is malformed or has an unknown format.",
        metadata: new Dictionary<string, object> { ["target"] = "query.cursor" });

    /// <summary>
    /// Создаёт ошибку размера страницы вне допустимого диапазона.
    /// </summary>
    /// <param name="pageSize">Запрошенный размер страницы.</param>
    /// <returns>
    /// Ошибка валидации с кодом <c>Set.PageSizeInvalid</c>
    /// и запрошенным, минимальным и максимальным размером в метаданных.
    /// </returns>
    public static Error PageSizeInvalid(int pageSize) => Error.Validation(
        code: "Set.PageSizeInvalid",
        description: $"Page size {pageSize} is outside the allowed range {SetPaginationConstraints.MinPageSize}..{SetPaginationConstraints.MaxPageSize}.",
        metadata: new Dictionary<string, object>
        {
            ["target"] = "query.pageSize",
            ["pageSize"] = pageSize,
            ["min"] = SetPaginationConstraints.MinPageSize,
            ["max"] = SetPaginationConstraints.MaxPageSize,
        });
}
