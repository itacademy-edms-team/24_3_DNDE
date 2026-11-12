using System.Text.Json;

namespace Sets.Application.Elements;

/// <summary>
/// Тело POST /api/sets/{setId}/elements.
/// </summary>
/// <param name="Type">Тип элемента; в CREATE-01 поддерживается только <c>flashcard</c>.</param>
/// <param name="Payload">Схема полей зависит от типа; разбор и проверка происходит в домене по контрактному типу.</param>
public sealed record CreateElementRequest(string? Type, JsonElement? Payload);

/// <summary>
/// Тело PUT /api/sets/{setId}/elements/{id}: полная замена содержимого.
/// Тип не передаётся - он неизменяем и берётся у существующего элемента.
/// </summary>
public sealed record UpdateElementRequest(JsonElement? Payload);

/// <summary>Тело PUT /api/sets/{setId}/elements/order: все активные id ровно один раз.</summary>
public sealed record ReorderElementsRequest(IReadOnlyList<Guid>? ElementIds);

/// <summary>Маршруты элементов набора: id набора всегда из пути, а не из тела.</summary>
public static class ElementRoute
{
    public const string Group = "/api/sets/{setId:guid}/elements";

    public const string ById = "{elementId:guid}";

    public const string Order = "order";
}
