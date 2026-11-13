using ErrorOr;

namespace Sets.Domain.Elements;

/// <summary>
/// Документное содержимое флеш-карточки: термин, определение и необязательные изображения сторон.
/// Изображения - ссылки на файлы другого модуля (uuid без внешнего ключа, ADR 0036).
/// </summary>
public sealed record FlashcardPayload(
    CardTerm Term,
    CardDefinition Definition,
    Guid? FrontImageId = null,
    Guid? BackImageId = null) : ElementPayload
{
    public override ElementType Type => ElementType.Flashcard;

    public static ErrorOr<FlashcardPayload> Create(
        string? term,
        string? definition,
        Guid? frontImageId = null,
        Guid? backImageId = null)
    {
        var termResult = CardTerm.Create(term);
        if (termResult.IsError)
            return termResult.Errors;

        var definitionResult = CardDefinition.Create(definition);
        if (definitionResult.IsError)
            return definitionResult.Errors;

        if (frontImageId == Guid.Empty || backImageId == Guid.Empty)
            return ElementErrors.ImageInvalid;

        return new FlashcardPayload(termResult.Value, definitionResult.Value, frontImageId, backImageId);
    }
}
