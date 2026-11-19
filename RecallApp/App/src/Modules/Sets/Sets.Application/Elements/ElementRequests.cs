using System.Text.Json;

namespace Sets.Application.Elements;

/// <summary>
/// Представляет тело запроса на создание элемента набора.
/// </summary>
/// <param name="Type">Контрактное имя типа элемента, например <c>flashcard</c>.</param>
/// <param name="Payload">Содержимое элемента; схема полей зависит от типа и проверяется в домене.</param>
public sealed record CreateElementRequest(string? Type, JsonElement? Payload);

/// <summary>
/// Представляет тело запроса на замену содержимого элемента.
/// </summary>
/// <param name="Payload">Новое содержимое по схеме типа элемента.</param>
/// <remarks>
/// Тип в теле не передаётся: он неизменяем и берётся у существующего элемента.
/// </remarks>
public sealed record UpdateElementRequest(JsonElement? Payload);

/// <summary>
/// Представляет тело запроса на перемещение элемента.
/// </summary>
/// <param name="PreviousId">Сосед перед элементом; <see langword="null"/> - начало набора.</param>
/// <param name="NextId">Сосед после элемента; <see langword="null"/> - конец набора.</param>
/// <remarks>
/// Хотя бы один сосед обязателен: без них непонятно, куда ставить элемент.
/// </remarks>
public sealed record MoveElementRequest(Guid? PreviousId, Guid? NextId);

/// <summary>
/// Предоставляет маршруты элементов набора.
/// </summary>
/// <remarks>
/// Идентификатор набора всегда приходит из пути, а не из тела запроса.
/// </remarks>
public static class ElementRoute
{
    public const string Group = "/api/sets/{setId:guid}/elements";

    public const string ById = "{elementId:guid}";

    public const string Move = "{elementId:guid}/move";
}
