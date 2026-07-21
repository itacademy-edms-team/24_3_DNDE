namespace EDMS1.CommandLog.Exceptions;

/// <summary>
/// Ошибка сущность не найдена.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="EntityNotFoundException"/> class.
/// </remarks>
/// <param name="entityName">See <see cref="EntityName"/>.</param>
/// <param name="fieldName">See <see cref="FieldName"/>.</param>
/// <param name="id">See <see cref="IdAsString"/>.</param>
[Serializable]
public class EntityNotFoundException(string entityName, string fieldName, string id)
    : NotFoundException(string.Format(ErrorMessages.EntityNotFound, entityName, fieldName, id))
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityNotFoundException"/> class.
    /// </summary>
    /// <param name="entityName">See <see cref="EntityName"/>.</param>
    /// <param name="fieldName">See <see cref="FieldName"/>.</param>
    /// <param name="id">Identifier of entity.</param>
    public EntityNotFoundException(string entityName, string fieldName, int id)
        : this(entityName, fieldName, id.ToString())
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityNotFoundException"/> class.
    /// </summary>
    /// <param name="entityName">See <see cref="EntityName"/>.</param>
    /// <param name="fieldName">See <see cref="FieldName"/>.</param>
    /// <param name="idAsString">Identifier of entity..</param>
    public EntityNotFoundException(string entityName, string fieldName, Guid id)
        : this(entityName, fieldName, id.ToString())
    { }

    /// <summary>
    /// Name of entity, which is not found.
    /// </summary>
    public string EntityName { get; private set; } = entityName;

    /// <summary>
    /// Name of entity's field, which is used for search of entity.
    /// </summary>
    public string FieldName { get; private set; } = fieldName;

    /// <summary>
    /// Identifier of entity as <see cref="string"/>.
    /// </summary>
    public string IdAsString { get; private set; } = id;
}
