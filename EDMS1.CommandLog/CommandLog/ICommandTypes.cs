namespace EDMS1.CommandLog;

/// <summary>
/// Интерфейс для списка логируемых команд.
/// </summary>
public interface ICommandTypes : IReadOnlyDictionary<string, Type>
{
}