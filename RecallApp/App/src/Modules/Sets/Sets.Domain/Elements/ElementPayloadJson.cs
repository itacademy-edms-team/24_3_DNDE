using System.Text.Json;
using ErrorOr;

namespace Sets.Domain.Elements;

/// <summary>
/// Сериализация payload элемента в JSON и обратно: единая точка для хранения и контракта.
/// <para>
/// Хранение (<see cref="ToStorageJson"/>): плоский объект с дискриминатором,
/// например <c>{"type":"flashcard","term":"…","definition":"…"}</c> - конвертер EF не видит
/// соседнюю колонку <c>type</c>, поэтому тип дублируется внутри документа.
/// </para>
/// <para>
/// Контракт (<see cref="ToContractJson"/>): только поля формата, без дискриминатора -
/// наружу тип отдаётся один раз, отдельным полем элемента.
/// </para>
/// </summary>
public static class ElementPayloadJson
{
    public static string ToStorageJson(ElementPayload payload) => payload switch
    {
        FlashcardPayload flashcard => JsonSerializer.Serialize(new
        {
            type = ElementTypeNames.Flashcard,
            term = flashcard.Term.Value,
            definition = flashcard.Definition.Value,
        }),
        _ => throw new ArgumentOutOfRangeException(nameof(payload), payload.GetType(), "Unknown payload type."),
    };

    public static string ToContractJson(ElementPayload payload) => payload switch
    {
        FlashcardPayload flashcard => JsonSerializer.Serialize(new
        {
            term = flashcard.Term.Value,
            definition = flashcard.Definition.Value,
        }),
        _ => throw new ArgumentOutOfRangeException(nameof(payload), payload.GetType(), "Unknown payload type."),
    };

    /// <summary>
    /// Восстанавливает элемент из БД.
    /// Повреждённый документ - <see cref="JsonException"/>: молча подставлять пустой payload нельзя, иначе невалидный элемент уйдёт в прохождение.
    /// </summary>
    public static ElementPayload FromStorageJson(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (!root.TryGetProperty("type", out var typeProperty)
            || !ElementTypeNames.TryParse(typeProperty.GetString(), out var type))
            throw new JsonException("Stored element payload has no known 'type' discriminator.");

        var result = Create(type, root);
        return result.IsError
            ? throw new JsonException($"Stored element payload is invalid: {result.FirstError.Code}.")
            : result.Value;
    }

    /// <summary>
    /// Собирает payload из JSON-объекта по типу элемента. 
    /// Используется и при чтении из БД, и при разборе тела запроса - правила проверки в обоих случаях одни и те же.
    /// </summary>
    public static ErrorOr<ElementPayload> Create(ElementType type, JsonElement payload) => type switch
    {
        ElementType.Flashcard => CreateFlashcard(payload),
        _ => ElementErrors.TypeUnsupported,
    };

    private static ErrorOr<ElementPayload> CreateFlashcard(JsonElement payload)
    {
        if (payload.ValueKind != JsonValueKind.Object)
            return ElementErrors.PayloadInvalid(ElementTypeNames.Flashcard);

        var term = payload.TryGetProperty("term", out var termProperty) ? termProperty.GetString() : null;
        var definition = payload.TryGetProperty("definition", out var definitionProperty)
            ? definitionProperty.GetString()
            : null;

        var result = FlashcardPayload.Create(term, definition);
        return result.IsError ? result.Errors : result.Value;
    }
}
