using Common.Domain.DomainEvents;
using ErrorOr;

namespace Sets.Domain.Sets;

/// <summary>
/// Представляет принадлежащий пользователю набор из элементов, помогающих запомнить материал.
/// </summary>
/// <remarks>
/// <para>Чужие наборы неотличимы от несуществующих:
/// операции с ними завершаются ошибкой <see cref="SetErrors.NotFound"/>.</para>
/// <para>Жизненный цикл: активен -> помечен на удаление -> удалён физически.
/// Пока набор помечен, но ещё не удалён, он недоступен для чтения и правки.</para>
/// <para><see cref="UpdatedAt"/> обновляется и при правке полей,
/// и при изменении содержимого (<see cref="MarkContentChanged"/>).</para>
/// </remarks>
public sealed class Set : HasDomainEventsBase
{
    private Set() { }

    /// <summary>
    /// Возвращает идентификатор набора.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Возвращает идентификатор доменного пользователя, которому принадлежит набор.
    /// </summary>
    public Guid OwnerId { get; private set; }

    /// <summary>
    /// Возвращает название набора.
    /// </summary>
    public SetTitle Title { get; private set; }

    /// <summary>
    /// Возвращает описание набора.
    /// </summary>
    /// <remarks>
    /// <see cref="SetDescription"/> хранит пустую строку, если описание не задано.
    /// </remarks>
    public SetDescription Description { get; private set; }

    /// <summary>
    /// Временная метка создания набора.
    /// </summary>
    /// <remarks>
    /// Задаётся сервисом.
    /// </remarks>
    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Временная метка, когда набор был помечен для удаления.
    /// </summary>
    /// <remarks>
    /// Такой набор и его элементы скрываются в клиенте и не показываются в большинстве взаимодействий на бэкенде.
    /// Удаление выполняет фоновый сервис <see cref="SetsPurgeService"/>.
    /// </remarks>
    public DateTime? DeletedAt { get; private set; }
    
    public static ErrorOr<Set> Create(Guid ownerId, string? title, string? description, DateTime utcNow)
    {
        if (ownerId == Guid.Empty)
            return SetErrors.UserUnresolved;

        var titleResult = SetTitle.Create(title);
        if (titleResult.IsError)
            return titleResult.Errors;

        ErrorOr<SetDescription> descriptionResult = ParseDescription(description);
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

        ErrorOr<SetDescription> descriptionResult = ParseDescription(description);
        if (descriptionResult.IsError)
            return descriptionResult.Errors;

        var newTitle = titleResult.Value;
        var newDescription = descriptionResult.Value;
        if (newTitle.Equals(Title) && newDescription.Equals(Description))
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

    private static ErrorOr<SetDescription> ParseDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return SetDescription.Empty;

        return SetDescription.Create(description);
    }
}
