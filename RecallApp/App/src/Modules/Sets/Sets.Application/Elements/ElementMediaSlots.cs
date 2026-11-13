using Media.Contracts;
using Sets.Domain.Elements;

namespace Sets.Application.Elements;

/// <summary>
/// Слоты использования изображений элементом: имена слотов задаёт Sets, смысл строки Media не разбирает
/// (онтология Media §6.4). Изменение картинки - это тот же слот с другим файлом.
/// </summary>
public static class ElementMediaSlots
{
    public const string Front = "front";
    public const string Back = "back";

    public static string ElementScope(Guid setId, Guid elementId) => $"set:{setId}:element:{elementId}";

    public static string SetScope(Guid setId) => $"set:{setId}";

    /// <summary>Текущий набор слотов элемента: только стороны с изображением.</summary>
    public static IReadOnlyList<MediaReferenceSlot> For(ElementPayload payload, string scope) => payload switch
    {
        FlashcardPayload card => Build(scope, card.FrontImageId, card.BackImageId),
        _ => [],
    };

    /// <summary>Идентификаторы файлов, которые надо проверить перед сохранением элемента.</summary>
    public static IReadOnlyCollection<Guid> ImageIds(ElementPayload payload) => payload switch
    {
        FlashcardPayload card => new[] { card.FrontImageId, card.BackImageId }
            .Where(mediaId => mediaId is not null)
            .Select(mediaId => mediaId!.Value)
            .Distinct()
            .ToArray(),
        _ => [],
    };

    private static IReadOnlyList<MediaReferenceSlot> Build(string scope, Guid? frontImageId, Guid? backImageId)
    {
        var slots = new List<MediaReferenceSlot>(2);

        if (frontImageId is { } front)
            slots.Add(new MediaReferenceSlot($"{scope}:{Front}", front));

        if (backImageId is { } back)
            slots.Add(new MediaReferenceSlot($"{scope}:{Back}", back));

        return slots;
    }
}
