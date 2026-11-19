using ErrorOr;

namespace Sets.Domain.Elements;

/// <summary>
/// Каталог ошибок элементов.
/// Формат <c>&lt;Сущность&gt;.&lt;Правило&gt;</c>.
/// </summary>
public static class ElementErrors
{
    /// <summary>Элемента нет, он удалён или принадлежит чужому набору: снаружи неотличимы (404).</summary>
    public static readonly Error NotFound = Error.NotFound(
        code: "Element.NotFound",
        description: "Element does not exist or is not accessible to the current user.");

    public static readonly Error SetRequired = Error.Validation(
        code: "Element.SetRequired",
        description: "Element must belong to a set.",
        metadata: new Dictionary<string, object> { ["target"] = "element.setId" });

    /// <summary>Ссылка на изображение не является корректным идентификатором файла.</summary>
    public static Error ImageInvalid => Error.Validation(
        code: "Element.ImageInvalid",
        description: "Image reference must be a non-empty file identifier.",
        metadata: new Dictionary<string, object> { ["target"] = "element.payload" });

    public static Error TypeUnsupported => Error.Validation(
        code: "Element.TypeUnsupported",
        description: "Only the flashcard element type is supported in this version.",
        metadata: new Dictionary<string, object> { ["target"] = "element.type" });

    public static readonly Error TypeRequired = Error.Validation(
        code: "Element.TypeRequired",
        description: "Element type must not be empty.",
        metadata: new Dictionary<string, object> { ["target"] = "element.type" });

    /// <summary>Тип элемента неизменяем: payload другого типа - это новый элемент, а не правка старого.</summary>
    public static Error TypeMismatch(ElementType current, ElementType incoming) => Error.Validation(
        code: "Element.TypeMismatch",
        description: $"Element type '{current.ToContractValue()}' cannot be replaced with '{incoming.ToContractValue()}'.",
        metadata: new Dictionary<string, object> { ["target"] = "element.type" });

    public static readonly Error PayloadRequired = Error.Validation(
        code: "Element.PayloadRequired",
        description: "Element payload must not be empty.",
        metadata: new Dictionary<string, object> { ["target"] = "element.payload" });

    /// <summary>Payload не той формы, которую ожидает тип элемента.</summary>
    public static Error PayloadInvalid(string type) => Error.Validation(
        code: "Element.PayloadInvalid",
        description: $"Payload must be a JSON object matching the '{type}' element schema.",
        metadata: new Dictionary<string, object> { ["target"] = "element.payload" });

    public static readonly Error TermRequired = Error.Validation(
        code: "Element.TermRequired",
        description: "Term must not be empty.",
        metadata: new Dictionary<string, object> { ["target"] = "element.payload.term" });

    public static Error TermTooLong(int length) => Error.Validation(
        code: "Element.TermTooLong",
        description: $"Term length {length} exceeds limit {ElementConstraints.TermMaxLength}.",
        metadata: new Dictionary<string, object>
        {
            ["target"] = "element.payload.term",
            ["max"] = ElementConstraints.TermMaxLength,
        });

    public static readonly Error DefinitionRequired = Error.Validation(
        code: "Element.DefinitionRequired",
        description: "Definition must not be empty.",
        metadata: new Dictionary<string, object> { ["target"] = "element.payload.definition" });

    public static Error DefinitionTooLong(int length) => Error.Validation(
        code: "Element.DefinitionTooLong",
        description: $"Definition length {length} exceeds limit {ElementConstraints.DefinitionMaxLength}.",
        metadata: new Dictionary<string, object>
        {
            ["target"] = "element.payload.definition",
            ["max"] = ElementConstraints.DefinitionMaxLength,
        });

    /// <summary>Ранг позиции пуст или не задан.</summary>
    public static Error PositionInvalid(string? position) => Error.Validation(
        code: "Element.PositionInvalid",
        description: $"Position rank '{position}' must not be empty.",
        metadata: new Dictionary<string, object> { ["target"] = "element.position" });

    /// <summary>Перестановка без соседей: непонятно, куда ставить элемент.</summary>
    public static readonly Error OrderNeighboursRequired = Error.Validation(
        code: "Element.OrderNeighboursRequired",
        description: "At least one neighbour (previous or next) is required to position the element.",
        metadata: new Dictionary<string, object> { ["target"] = "body" });

    /// <summary>Соседи заданы так, что позиция между ними не определена.</summary>
    public static Error OrderInvalid(string reason) => Error.Validation(
        code: "Element.OrderInvalid",
        description: $"Order neighbours are invalid: {reason}.",
        metadata: new Dictionary<string, object> { ["target"] = "body" });

    /// <summary>Указанный сосед не найден среди активных элементов набора.</summary>
    public static Error OrderNeighbourNotFound(Guid elementId) => Error.Validation(
        code: "Element.OrderNeighbourNotFound",
        description: $"Neighbour element '{elementId}' is not an active element of this set.",
        metadata: new Dictionary<string, object> { ["target"] = "body" });

    /// <summary>Курсор пагинации не разбирается или не того формата.</summary>
    public static readonly Error CursorInvalid = Error.Validation(
        code: "Element.CursorInvalid",
        description: "Cursor is malformed or has an unknown format.",
        metadata: new Dictionary<string, object> { ["target"] = "query.cursor" });

    public static Error PageSizeInvalid(int pageSize) => Error.Validation(
        code: "Element.PageSizeInvalid",
        description: $"Page size {pageSize} is outside the allowed range {ElementListConstraints.MinPageSize}..{ElementListConstraints.MaxPageSize}.",
        metadata: new Dictionary<string, object>
        {
            ["target"] = "query.pageSize",
            ["min"] = ElementListConstraints.MinPageSize,
            ["max"] = ElementListConstraints.MaxPageSize,
        });
}
