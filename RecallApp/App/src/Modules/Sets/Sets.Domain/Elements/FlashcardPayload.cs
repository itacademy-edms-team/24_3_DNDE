using ErrorOr;

namespace Sets.Domain.Elements;

/// <summary>
/// Документное содержимое флеш-карточки: термин и определение.
/// </summary>
public sealed record FlashcardPayload(CardTerm Term, CardDefinition Definition) : ElementPayload
{
    public override ElementType Type => ElementType.Flashcard;

    public static ErrorOr<FlashcardPayload> Create(string? term, string? definition)
    {
        var termResult = CardTerm.Create(term);
        if (termResult.IsError)
            return termResult.Errors;

        var definitionResult = CardDefinition.Create(definition);
        if (definitionResult.IsError)
            return definitionResult.Errors;

        return new FlashcardPayload(termResult.Value, definitionResult.Value);
    }
}
