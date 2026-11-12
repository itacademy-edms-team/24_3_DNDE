using System.Text.Json;
using Sets.Domain.Elements;

namespace Sets.Application.Elements;

/// <summary>
/// Представление элемента набора в ответе API.
/// Тип отдаётся стабильной строкой, payload - только полями формата и без дискриминатора.
/// </summary>
public sealed record ElementResponse(
    Guid Id,
    string Type,
    JsonElement Payload,
    int Position,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static ElementResponse From(Element element) => new(
        element.Id,
        element.Type.ToContractValue(),
        ToJsonElement(ElementPayloadJson.ToContractJson(element.Payload)),
        element.Position,
        element.CreatedAt,
        element.UpdatedAt);

    private static JsonElement ToJsonElement(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
