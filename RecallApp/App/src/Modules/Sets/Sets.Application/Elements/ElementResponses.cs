using System.Text.Json;
using Media.Contracts;
using Sets.Domain.Elements;

namespace Sets.Application.Elements;

/// <summary>
/// Представляет элемент набора в ответе API.
/// </summary>
/// <remarks>
/// Тип передаётся стабильной строкой отдельным полем; в payload его нет.
/// Изображения приходят подписанными ссылками на варианты, если их запросили вместе с элементом.
/// </remarks>
/// <param name="Id">Идентификатор элемента.</param>
/// <param name="Type">Контрактное имя типа элемента.</param>
/// <param name="Payload">Содержимое элемента по схеме типа, без дискриминатора.</param>
/// <param name="Images">Карта «сторона -> варианты»; <see langword="null"/>, если изображений нет
/// или ссылки не запрашивались вместе с элементом.</param>
/// <param name="CreatedAt">Момент создания в UTC.</param>
/// <param name="UpdatedAt">Момент последнего изменения в UTC.</param>
public sealed record ElementResponse(
    Guid Id,
    string Type,
    JsonElement Payload,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, MediaImageLink>>? Images,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    /// <summary>
    /// Создаёт представление элемента для ответа API.
    /// </summary>
    /// <param name="element">Доменный элемент.</param>
    /// <param name="links">Подписанные ссылки на файлы; <see langword="null"/> - не запрашивались.</param>
    /// <returns>Представление элемента.</returns>
    public static ElementResponse From(
        Element element,
        IReadOnlyDictionary<Guid, MediaFileLinks>? links = null) => new(
        element.Id,
        element.Type.ToContractValue(),
        ToJsonElement(ElementPayloadJson.ToContractJson(element.Payload)),
        BuildImages(element.Payload, links),
        element.CreatedAt,
        element.UpdatedAt);

    /// <summary>
    /// Собирает карту «сторона -> варианты». Если нет ссылок, то нет и поля, чтобы клиент отличал
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
