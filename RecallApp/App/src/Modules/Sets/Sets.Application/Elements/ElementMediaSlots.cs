using Media.Contracts;
using Sets.Domain.Elements;

namespace Sets.Application.Elements;

/// <summary>
/// Предоставляет слоты использования изображений элементом.
/// </summary>
/// <remarks>
/// <para>Имена слотов задаёт Sets; Media хранит их как непрозрачные строки и не разбирает
/// (онтология Media §6.4).</para>
/// <para>Замена картинки - это тот же слот с другим файлом: ссылка на старый файл снимается,
/// новый ставится, отдельной отвязки не нужно.</para>
/// <para>Области (scope) строятся с префиксом набора, поэтому все ссылки набора можно снять
/// одним сообщением - так и работает удаление набора.</para>
/// </remarks>
public static class ElementMediaSlots
{
    /// <summary>Слот лицевой стороны карточки.</summary>
    public const string Front = "front";
    
    /// <summary>Слот оборотной стороны карточки.</summary>
    public const string Back = "back";

    /// <summary>
    /// Возвращает область ссылок одного элемента.
    /// </summary>
    /// <param name="setId">Идентификатор набора.</param>
    /// <param name="elementId">Идентификатор элемента.</param>
    /// <returns>Префикс ссылок элемента вида <c>set:{setId}:element:{elementId}</c>.</returns>
    public static string ElementScope(Guid setId, Guid elementId) => $"set:{setId}:element:{elementId}";

    /// <summary>
    /// Возвращает область ссылок всех элементов набора.
    /// </summary>
    /// <param name="setId">Идентификатор набора.</param>
    /// <returns>Префикс ссылок набора вида <c>set:{setId}</c>.</returns>
    public static string SetScope(Guid setId) => $"set:{setId}";

    /// <summary>
    /// Возвращает текущие слоты элемента - только стороны, у которых есть изображение.
    /// </summary>
    /// <param name="payload">Содержимое элемента.</param>
    /// <param name="scope">Область ссылок элемента из <see cref="ElementScope"/>.</param>
    /// <returns>Слоты для синхронизации с Media; пустой список, если изображений нет.</returns>
    public static IReadOnlyList<MediaReferenceSlot> For(ElementPayload payload, string scope) => payload switch
    {
        FlashcardPayload card => Build(scope, card.FrontImageId, card.BackImageId),
        _ => [],
    };

    /// <summary>
    /// Возвращает идентификаторы файлов, которые нужно проверить перед сохранением элемента.
    /// </summary>
    /// <param name="payload">Содержимое элемента.</param>
    /// <returns>Уникальные идентификаторы изображений без пустых значений.</returns>
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
