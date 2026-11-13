using System.Text.Json;
using Media.Contracts;
using Sets.Domain.Elements;

namespace Sets.Application.Elements;

/// <summary>
/// Представление элемента набора в ответе API.
/// Тип отдаётся стабильной строкой, payload - только полями формата и без дискриминатора,
/// изображения - подписанными ссылками на варианты (если запрошены вместе с элементом).
/// </summary>
public sealed record ElementResponse(
    Guid Id,
    string Type,
    JsonElement Payload,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, MediaImageLink>>? Images,
    int Position,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static ElementResponse From(
        Element element,
        IReadOnlyDictionary<Guid, MediaFileLinks>? links = null) => new(
        element.Id,
        element.Type.ToContractValue(),
        ToJsonElement(ElementPayloadJson.ToContractJson(element.Payload)),
        BuildImages(element.Payload, links),
        element.Position,
        element.CreatedAt,
        element.UpdatedAt);

    /// <summary>
    /// Собирает карту «сторона -> варианты»: ссылок нет - поля нет, чтобы клиент отличал
    /// отсутствие изображения от неготового файла.
    /// </summary>
    private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, MediaImageLink>>? BuildImages(
        ElementPayload payload,
        IReadOnlyDictionary<Guid, MediaFileLinks>? links)
    {
        if (links is null || links.Count == 0)
            return null;

        if (payload is not FlashcardPayload card)
            return null;

        var images = new Dictionary<string, IReadOnlyDictionary<string, MediaImageLink>>(2);

        AddSlot(images, ElementMediaSlots.Front, card.FrontImageId, links);
        AddSlot(images, ElementMediaSlots.Back, card.BackImageId, links);

        return images.Count == 0 ? null : images;
    }

    private static void AddSlot(
        IDictionary<string, IReadOnlyDictionary<string, MediaImageLink>> images,
        string slot,
        Guid? mediaId,
        IReadOnlyDictionary<Guid, MediaFileLinks> links)
    {
        if (mediaId is { } id && links.TryGetValue(id, out var file))
            images[slot] = file.Variants;
    }

    private static JsonElement ToJsonElement(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
