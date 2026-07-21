using System.Collections.ObjectModel;

namespace EDMS1.CommandLog.Models;

/// <summary>
/// Словарь для хранения списка логируемых команд.
/// </summary>
public class CommandTypes : ReadOnlyDictionary<string, Type>, ICommandTypes
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandTypes"/> class.
    /// </summary>
    public CommandTypes(IDictionary<string, Type> dictionary)
        : base(dictionary)
    {
    }
}