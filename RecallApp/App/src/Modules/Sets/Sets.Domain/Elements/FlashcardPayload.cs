using ErrorOr;

namespace Sets.Domain.Elements;

/// <summary>
/// Представляет содержимое флеш-карточки: термин, определение и необязательные изображения на сторонах.
/// </summary>
/// <param name="Term">Термин (лицевая сторона).</param>
/// <param name="Definition">Определение (оборотная сторона).</param>
/// <param name="FrontImageId">Идентификатор изображения лицевой стороны;
/// <see langword="null"/> - без изображения.</param>
/// <param name="BackImageId">Идентификатор изображения оборотной стороны;
/// <see langword="null"/> - без изображения.</param>
/// <remarks>
/// Изображения - ссылки на файлы модуля Media: идентификаторы без внешнего ключа (ADR 0036).
/// Владение и готовность файла проверяет модуль Media.
/// </remarks>
public sealed record FlashcardPayload(
    CardTerm Term,
    CardDefinition Definition,
    Guid? FrontImageId = null,
    Guid? BackImageId = null) : ElementPayload
{
    /// <inheritdoc/>
    public override ElementType Type => ElementType.Flashcard;

    /// <summary>
    /// Создаёт содержимое флеш-карточки, проверяя термин и определение.
    /// </summary>
    /// <param name="term">Сырой термин; пробелы по краям отбрасываются.</param>
    /// <param name="definition">Сырое определение; пробелы по краям отбрасываются.</param>
    /// <param name="frontImageId">Идентификатор изображения лицевой стороны или <see langword="null"/>.</param>
    /// <param name="backImageId">Идентификатор изображения оборотной стороны или <see langword="null"/>.</param>
    /// <returns>
    /// Содержимое карточки либо ошибка:
    /// <see cref="ElementErrors.TermRequired"/>, <see cref="ElementErrors.TermTooLong"/>,
    /// <see cref="ElementErrors.DefinitionRequired"/>, <see cref="ElementErrors.DefinitionTooLong"/>
    /// <see cref="ElementErrors.ImageInvalid"/>.
    /// </returns>
    /// <remarks>
    /// Пустой <see cref="Guid"/> как идентификатор изображения считается ошибкой ввода,
    /// а не отсутствием изображения: для показа отсутствия используется <see langword="null"/>.
    /// </remarks>
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
