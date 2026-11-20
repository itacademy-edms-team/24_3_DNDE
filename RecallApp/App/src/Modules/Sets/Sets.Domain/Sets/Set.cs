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
/// <para><see cref="UpdatedAt"/> обновляется и при правке полей, и при любом изменении состава
/// (создание, правка, удаление, перестановка элементов), хотя сами элементы при перестановке
/// своих меток не меняют.</para>
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
    /// Возвращает момент создания набора в UTC.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Возвращает момент последнего изменения набора в UTC.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Возвращает момент мягкого удаления набора в UTC или <see langword="null"/>, если набор не удалён.
    /// </summary>
    /// <remarks>
    /// Пометка идемпотентна: нельзя изменить состояние уже установленной пометки.
    /// Физически строку удаляет фоновая чистка по истечении срока хранения.
    /// </remarks>
    public DateTime? DeletedAt { get; private set; }
    
    /// <summary>
    /// Создаёт новый набор с указанными полями.
    /// </summary>
    /// <param name="ownerId">Идентификатор владельца набора.</param>
    /// <param name="title">Название набора.</param>
    /// <param name="description">Описание набора; <see langword="null"/> и пробельная строка означают отсутствие описания.</param>
    /// <param name="utcNow">Момент создания набора в UTC. Задаётся вызывающим кодом.</param>
    /// <returns>
    /// Новый набор в состоянии «активен» либо ошибка:
    /// <see cref="SetErrors.UserUnresolved"/>, <see cref="SetErrors.TitleRequired"/>,
    /// <see cref="SetErrors.TitleTooLong"/>, <see cref="SetErrors.DescriptionTooLong"/>.
    /// </returns>
    public static ErrorOr<Set> Create(Guid ownerId, string? title, string? description, DateTime utcNow)
    {
        if (ownerId == Guid.Empty)
            return SetErrors.UserUnresolved;

        var titleResult = SetTitle.Create(title);
        if (titleResult.IsError)
            return titleResult.Errors;

        var descriptionResult = SetDescription.Create(description);
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
    /// Заменяет название и описание набора целиком.
    /// </summary>
    /// <remarks>
    /// Если после нормализации значения совпадают с текущими,
    /// набор не меняется и <see cref="UpdatedAt"/> не обновляется.
    /// </remarks>
    /// <param name="title">Название набора.</param>
    /// <param name="description">Описание набора; <see langword="null"/>
    /// и пробельная строка означают отсутствие описания.</param>
    /// <param name="utcNow">Текущий момент в UTC.
    /// Задаётся вызывающим кодом и применяется, только если значения полей изменились.</param>
    /// <returns><see cref="ErrorOr.Updated"/>, если поля приведены к переданным значениям
    /// (в том числе когда они уже совпадали), иначе ошибка:
    /// <see cref="SetErrors.TitleRequired"/>, <see cref="SetErrors.TitleTooLong"/>,
    /// <see cref="SetErrors.DescriptionTooLong"/>.
    /// </returns>
    public ErrorOr<Updated> Replace(string? title, string? description, DateTime utcNow)
    {
        var titleResult = SetTitle.Create(title);
        if (titleResult.IsError)
            return titleResult.Errors;

        var descriptionResult = SetDescription.Create(description);
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
    /// Помечает набор как удалённый.
    /// </summary>
    /// <remarks>
    /// При первой пометке обновляет <see cref="UpdatedAt"/>.
    /// </remarks>
    /// <param name="utcNow">Текущий момент в UTC, который станет меткой. Задаётся вызывающим кодом.</param>
    public void MarkDeleted(DateTime utcNow)
    {
        if (DeletedAt is not null)
            return;

        DeletedAt = utcNow;
        UpdatedAt = utcNow;
    }

    /// <summary>
    /// Отмечает изменение состава набора, обновляя момент его последнего изменения.
    /// </summary>
    /// <param name="utcNow">Текущий момент в UTC. Задаётся вызывающим кодом.</param>
    public void MarkElementsChanged(DateTime utcNow) => UpdatedAt = utcNow;
}
