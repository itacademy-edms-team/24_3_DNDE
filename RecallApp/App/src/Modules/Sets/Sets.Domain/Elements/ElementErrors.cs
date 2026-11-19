using ErrorOr;

namespace Sets.Domain.Elements;

/// <summary>
/// Предоставляет каталог ошибок агрегата элемента.
/// </summary>
/// <remarks>
/// Код ошибки является частью контракта для клиента (ADR 0038)
/// и имеет формат <c>&lt;Сущность&gt;.&lt;Правило&gt;</c>: по нему выбирается текст для пользователя.
/// Описание ошибки предназначено для логов и в интерфейс не попадает.
/// Метаданные используются в логах и внутренних обработчиках;
/// в ответ ProblemDetails сейчас уходит только <c>target</c>.
/// </remarks>
public static class ElementErrors
{
    /// <summary>
    /// Элемента нет или он принадлежит чужому набору.
    /// </summary>
    public static readonly Error NotFound = Error.NotFound(
        code: "Element.NotFound",
        description: "Element does not exist or is not accessible to the current user.");

    /// <summary>
    /// Набор не задан: элемент не может существовать вне набора.
    /// </summary>
    public static readonly Error SetRequired = Error.Validation(
        code: "Element.SetRequired",
        description: "Element must belong to a set.",
        metadata: new Dictionary<string, object> { ["target"] = "element.setId" });

    /// <summary>
    /// Ссылка на изображение не является корректным идентификатором файла.
    /// </summary>
    public static readonly Error ImageInvalid = Error.Validation(
        code: "Element.ImageInvalid",
        description: "Image reference must be a non-empty file identifier.",
        metadata: new Dictionary<string, object> { ["target"] = "element.payload" });

    /// <summary>
    /// Тип элемента неизвестен или не поддерживается в этой версии.
    /// </summary>
    public static readonly Error TypeUnsupported = Error.Validation(
        code: "Element.TypeUnsupported",
        description: "Only the flashcard element type is supported in this version.",
        metadata: new Dictionary<string, object> { ["target"] = "element.type" });

    /// <summary>
    /// Тип элемента не задан или пуст.
    /// </summary>
    public static readonly Error TypeRequired = Error.Validation(
        code: "Element.TypeRequired",
        description: "Element type must not be empty.",
        metadata: new Dictionary<string, object> { ["target"] = "element.type" });

    /// <summary>
    /// Создаёт ошибку попытки сменить тип существующего элемента.
    /// </summary>
    /// <param name="current">Текущий тип элемента.</param>
    /// <param name="incoming">Тип из нового содержимого.</param>
    /// <returns>Ошибка валидации с кодом <c>Element.TypeMismatch</c>.</returns>
    public static Error TypeMismatch(ElementType current, ElementType incoming) => Error.Validation(
        code: "Element.TypeMismatch",
        description: $"Element type '{current.ToContractValue()}' cannot be replaced with '{incoming.ToContractValue()}'.",
        metadata: new Dictionary<string, object> { ["target"] = "element.type" });

    /// <summary>
    /// Содержимое элемента не задано.
    /// </summary>
    public static readonly Error PayloadRequired = Error.Validation(
        code: "Element.PayloadRequired",
        description: "Element payload must not be empty.",
        metadata: new Dictionary<string, object> { ["target"] = "element.payload" });

    /// <summary>
    /// Создаёт ошибку несоответствия содержимого схеме типа.
    /// </summary>
    /// <param name="type">Контрактное имя типа элемента.</param>
    /// <returns>Ошибка валидации с кодом <c>Element.PayloadInvalid</c>.</returns>
    public static Error PayloadInvalid(string type) => Error.Validation(
        code: "Element.PayloadInvalid",
        description: $"Payload must be a JSON object matching the '{type}' element schema.",
        metadata: new Dictionary<string, object> { ["target"] = "element.payload" });

    /// <summary>
    /// Термин не задан или состоит из пробелов.
    /// </summary>
    public static readonly Error TermRequired = Error.Validation(
        code: "Element.TermRequired",
        description: "Term must not be empty.",
        metadata: new Dictionary<string, object> { ["target"] = "element.payload.term" });

    /// <summary>
    /// Создаёт ошибку превышения допустимой длины термина.
    /// </summary>
    /// <param name="length">Фактическая длина после нормализации.</param>
    /// <returns>Ошибка валидации с кодом <c>Element.TermTooLong</c>
    /// и фактической и максимальной длиной в метаданных.</returns>
    public static Error TermTooLong(int length) => Error.Validation(
        code: "Element.TermTooLong",
        description: $"Term length {length} exceeds limit {ElementConstraints.TermMaxLength}.",
        metadata: new Dictionary<string, object>
        {
            ["target"] = "element.payload.term",
            ["length"] = length,
            ["max"] = ElementConstraints.TermMaxLength,
        });

    /// <summary>
    /// Определение не задано или состоит из пробелов.
    /// </summary>
    public static readonly Error DefinitionRequired = Error.Validation(
        code: "Element.DefinitionRequired",
        description: "Definition must not be empty.",
        metadata: new Dictionary<string, object> { ["target"] = "element.payload.definition" });

    /// <summary>
    /// Создаёт ошибку превышения допустимой длины определения.
    /// </summary>
    /// <param name="length">Фактическая длина после нормализации.</param>
    /// <returns>Ошибка валидации с кодом <c>Element.DefinitionTooLong</c>
    /// и фактической и максимальной длиной в метаданных.</returns>
    public static Error DefinitionTooLong(int length) => Error.Validation(
        code: "Element.DefinitionTooLong",
        description: $"Definition length {length} exceeds limit {ElementConstraints.DefinitionMaxLength}.",
        metadata: new Dictionary<string, object>
        {
            ["target"] = "element.payload.definition",
            ["length"] = length,
            ["max"] = ElementConstraints.DefinitionMaxLength,
        });

    /// <summary>
    /// Перестановка без соседей: непонятно, куда ставить элемент.
    /// </summary>
    public static readonly Error OrderNeighboursRequired = Error.Validation(
        code: "Element.OrderNeighboursRequired",
        description: "At least one neighbour (previous or next) is required to position the element.",
        metadata: new Dictionary<string, object> { ["target"] = "body" });

    /// <summary>
    /// Создаёт ошибку некорректных соседей при перестановке.
    /// </summary>
    /// <param name="reason">Причина для логов и отладки.</param>
    /// <returns>Ошибка валидации с кодом <c>Element.OrderInvalid</c>.</returns>
    public static Error OrderInvalid(string reason) => Error.Validation(
        code: "Element.OrderInvalid",
        description: $"Order neighbours are invalid: {reason}.",
        metadata: new Dictionary<string, object> { ["target"] = "body" });

    /// <summary>
    /// Создаёт ошибку поиска соседа среди активных элементов набора.
    /// </summary>
    /// <param name="elementId">Идентификатор, которого нет среди активных элементов набора.</param>
    /// <returns>Ошибка валидации с кодом <c>Element.OrderNeighbourNotFound</c>.</returns>
    public static Error OrderNeighbourNotFound(Guid elementId) => Error.Validation(
        code: "Element.OrderNeighbourNotFound",
        description: $"Neighbour element '{elementId}' is not an active element of this set.",
        metadata: new Dictionary<string, object> { ["target"] = "body" });

    /// <summary>
    /// Курсор пагинации не разбирается или не того формата.
    /// </summary>
    public static readonly Error CursorInvalid = Error.Validation(
        code: "Element.CursorInvalid",
        description: "Cursor is malformed or has an unknown format.",
        metadata: new Dictionary<string, object> { ["target"] = "query.cursor" });

    /// <summary>
    /// Создаёт ошибку размера страницы вне допустимого диапазона.
    /// </summary>
    /// <param name="pageSize">Запрошенный размер страницы.</param>
    /// <returns>Ошибка валидации с кодом <c>Element.PageSizeInvalid</c>
    /// и запрошенным, минимальным и максимальным размером в метаданных.</returns>
    public static Error PageSizeInvalid(int pageSize) => Error.Validation(
        code: "Element.PageSizeInvalid",
        description: $"Page size {pageSize} is outside the allowed range {ElementListConstraints.MinPageSize}..{ElementListConstraints.MaxPageSize}.",
        metadata: new Dictionary<string, object>
        {
            ["target"] = "query.pageSize",
            ["pageSize"] = pageSize,
            ["min"] = ElementListConstraints.MinPageSize,
            ["max"] = ElementListConstraints.MaxPageSize,
        });
}
