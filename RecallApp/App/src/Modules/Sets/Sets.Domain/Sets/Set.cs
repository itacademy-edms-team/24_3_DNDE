using Common.Domain.DomainEvents;
using ErrorOr;

namespace Sets.Domain.Sets;

/// <summary>
/// Набор - коллекция элементов, принадлежащая одному пользователю (владельцу).
/// </summary>
public sealed class Set : HasDomainEventsBase
{
    private Set() { }

    public Guid Id { get; private set; }

    /// <summary>
    /// Id доменного пользователя из accounts.
    /// </summary>
    public Guid OwnerId { get; private set; }

    public SetTitle Title { get; private set; }

    /// <summary>
    /// Описание необязательно: null -> «описания нет».
    /// </summary>
    public SetDescription? Description { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Метка мягкого удаления; null -> набор активен.
    /// </summary>
    public DateTime? DeletedAt { get; private set; }

    public static ErrorOr<Set> Create(Guid ownerId, string? title, string? description, DateTime utcNow)
    {
        if (ownerId == Guid.Empty)
            return SetErrors.UserUnresolved;

        var titleResult = SetTitle.Create(title);
        if (titleResult.IsError)
            return titleResult.Errors;

        ErrorOr<SetDescription?> descriptionResult = ParseDescription(description);
        if (descriptionResult.IsError)
            return descriptionResult.Errors;

        return new Set
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            Title = titleResult.Value,
            Description = descriptionResult.Value,
            CreatedAt = utcNow,
            UpdatedAt = utcNow,
        };
    }

    /// <summary>
    /// Полная замена пользовательских полей.
    /// </summary>
    public ErrorOr<Updated> Replace(string? title, string? description, DateTime utcNow)
    {
        var titleResult = SetTitle.Create(title);
        if (titleResult.IsError)
            return titleResult.Errors;

        ErrorOr<SetDescription?> descriptionResult = ParseDescription(description);
        if (descriptionResult.IsError)
            return descriptionResult.Errors;

        var newTitle = titleResult.Value;
        var newDescription = descriptionResult.Value;
        if (newTitle.Equals(Title) && Nullable.Equals(newDescription, Description))
            return Result.Updated;

        Title = newTitle;
        Description = newDescription;
        UpdatedAt = utcNow;
        return Result.Updated;
    }

    /// <summary>
    /// Помечает набор, что он удалён и его следует позже удалить.
    /// </summary>
    public void MarkDeleted(DateTime utcNow)
    {
        if (DeletedAt is not null)
            return;

        DeletedAt = utcNow;
        UpdatedAt = utcNow;
    }

    /// <summary>
    /// Обновляет временную метку последнего изменения на utcNow.
    /// Изменение контента (элементов) набора считается редактированием самого набора.
    /// </summary>
    public void MarkContentChanged(DateTime utcNow) => UpdatedAt = utcNow;

    private static ErrorOr<SetDescription?> ParseDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return (SetDescription?)null;

        var result = SetDescription.Create(description);
        return result.IsError ? result.Errors : (SetDescription?)result.Value;
    }
}
