using System.Text.Json;
using ErrorOr;

namespace Sets.Domain.Elements;

/// <summary>
/// Предоставляет сериализацию содержимого элемента в JSON и обратно.
/// </summary>
/// <remarks>
/// <para>Один и тот же разбор работает и на чтении из БД, и на приёме тела запроса.</para>
/// <para>Для хранения используется плоский объект с дискриминатором <c>type</c>: EF-конвертер не видит
/// соседний столбец, поэтому тип дублируется внутри документа.</para>
/// <para>В ответе API дискриминатора в payload нет: тип элемента приходит отдельным полем.</para>
/// </remarks>
public static class ElementPayloadJson
{
    /// <summary>
    /// Сериализует содержимое для хранения в столбце <c>payload</c>.
    /// </summary>
    /// <param name="payload">Содержимое элемента.</param>
    /// <returns>JSON-объект с дискриминатором <c>type</c>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Для этого типа содержимого нет ветки сериализации: добавлен новый наследник без правила обработки.
    /// </exception>
    public static string ToStorageJson(ElementPayload payload) => payload switch
    {
        FlashcardPayload flashcard => JsonSerializer.Serialize(new
        {
            type = ElementTypeNames.Flashcard,
            term = flashcard.Term.Value,
            definition = flashcard.Definition.Value,
            frontImageId = flashcard.FrontImageId,
            backImageId = flashcard.BackImageId,
        }),
        _ => throw new ArgumentOutOfRangeException(nameof(payload), payload.GetType(), "Unknown payload type."),
    };

    /// <summary>
    /// Сериализует содержимое для ответа API.
    /// </summary>
    /// <param name="payload">Содержимое элемента.</param>
    /// <returns>JSON-объект только с полями формата, без дискриминатора.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Для этого типа содержимого нет ветки сериализации: добавлен новый наследник без правила обработки.
    /// </exception>
    public static string ToContractJson(ElementPayload payload) => payload switch
    {
        FlashcardPayload flashcard => JsonSerializer.Serialize(new
        {
            term = flashcard.Term.Value,
            definition = flashcard.Definition.Value,
            frontImageId = flashcard.FrontImageId,
            backImageId = flashcard.BackImageId,
        }),
        _ => throw new ArgumentOutOfRangeException(nameof(payload), payload.GetType(), "Unknown payload type."),
    };

    /// <summary>
    /// Восстанавливает содержимое из документа в столбце <c>payload</c>.
    /// </summary>
    /// <param name="json">Документ из хранилища.</param>
    /// <returns>Восстановленное содержимое.</returns>
    /// <exception cref="JsonException">
    /// Документ повреждён, не содержит дискриминатора или не проходит проверку полей.
    /// </exception>
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
    /// Создаёт содержимое из JSON-объекта по типу элемента.
    /// </summary>
    /// <param name="type">Тип элемента, определяющий схему полей.</param>
    /// <param name="payload">JSON-объект с полями формата.</param>
    /// <returns>
    /// Содержимое элемента либо ошибка:
    /// <see cref="ElementErrors.TypeUnsupported"/>, <see cref="ElementErrors.PayloadInvalid"/>,
    /// <see cref="ElementErrors.TermRequired"/>, <see cref="ElementErrors.TermTooLong"/>,
    /// <see cref="ElementErrors.DefinitionRequired"/>, <see cref="ElementErrors.DefinitionTooLong"/>
    /// или <see cref="ElementErrors.ImageInvalid"/>.
    /// </returns>
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

        var frontImage = ReadImageId(payload, "frontImageId");
        if (frontImage.IsError)
            return frontImage.Errors;

        var backImage = ReadImageId(payload, "backImageId");
        if (backImage.IsError)
            return backImage.Errors;

        var result = FlashcardPayload.Create(term, definition, frontImage.Value, backImage.Value);
        return result.IsError ? result.Errors : result.Value;
    }

    /// <summary>Читает необязательный идентификатор изображения: отсутствие поля и null равнозначны.</summary>
    private static ErrorOr<Guid?> ReadImageId(JsonElement payload, string propertyName)
    {
        if (!payload.TryGetProperty(propertyName, out var property) || property.ValueKind == JsonValueKind.Null)
            return (Guid?)null;

        return property.ValueKind == JsonValueKind.String && property.TryGetGuid(out var mediaId)
            ? mediaId
            : ElementErrors.ImageInvalid;
    }
}
