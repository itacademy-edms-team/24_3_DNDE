using System.Collections.Frozen;
using System.Reflection;
using EDMS1.CommandLog.Commands;
using Microsoft.Extensions.Logging;

namespace EDMS1.CommandLog.Resolvers;

/// <summary>
/// Резолвер для поиска команд по имени во всех загруженных в память нединамических сборках.
/// </summary>
internal sealed class CommandTypeResolver
{
    private readonly Lazy<IReadOnlyDictionary<string, Type>> _typeMap;
    private readonly ILogger<CommandTypeResolver> _logger;
    
    public CommandTypeResolver(ILogger<CommandTypeResolver> logger)
    {
        _typeMap = new Lazy<IReadOnlyDictionary<string, Type>>(BuildMap);
        _logger = logger;
    }
    
    /// <summary>
    /// Возвращает тип команды по её имени.
    /// </summary>
    /// <param name="typeName"><see cref="Type.Name"/> команды.</param>
    /// <returns>Найденный тип.</returns>
    /// <exception cref="InvalidOperationException">Тип команды с таким именем не найден.</exception>
    public Type Resolve(string typeName)
    {
        return _typeMap.Value.TryGetValue(typeName, out var type)
            ? type 
            : throw new InvalidOperationException($"Command type '{typeName}' not found.");
    }
    
    /// <summary>
    /// Строит карту типов команд по всем загруженным в памяти assembly.
    /// </summary>
    /// <returns>Построенный словарь вида "ИмяКоманды:ТипКоманды".</returns>
    /// <exception cref="InvalidOperationException">Найдены дубликаты имён команд.</exception>
    private FrozenDictionary<string, Type> BuildMap()
    {
        // Используем метод получения уже загруженных в память сборок.
        // MediatR гарантирует на этапе запуска подгрузку всех зарегистрированных команд в память.
        var assembliesWithTypes  = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => !a.IsDynamic)
            .Select(a =>
        {
            var commandTypesInAssembly = SafeGetTypes(a)
                .Where(t => t.IsAssignableTo(typeof(ICommand)) && t.IsClass && !t.IsAbstract)
                .ToArray();

            if (commandTypesInAssembly.Length > 0)
            {
                _logger.LogDebug("Found {CommandCount} command types in assembly {Assembly}", 
                    commandTypesInAssembly.Length, a.FullName);
            }
            
            return commandTypesInAssembly;
        }).ToArray();
        var commandTypes = assembliesWithTypes
            .SelectMany(t => t)
            .ToArray();
        
        _logger.LogInformation("Found {CommandCount} command types in {AssemblyCount} assemblies", 
            commandTypes.Length, assembliesWithTypes.Count(a => a.Length > 0));
        
        var duplicateTypes = commandTypes
            .GroupBy(c => c.Name)
            .Where(g => g.Count() > 1)
            .OrderBy(g => g.Key)
            .ToArray();
        if (duplicateTypes.Length > 0)
        {
            var details = string.Join("; ", duplicateTypes
                .Select(g => $"{g.Key}: [{string.Join(", ", g.Select(t => t.FullName))}]"));
            throw new InvalidOperationException($"Duplicate command names found: {details}.");
        }

        return commandTypes.ToFrozenDictionary(t => t.Name);
    }
    
    /// <summary>
    /// Безопасно достаёт все типы из сборки.
    /// Учитывает ситуации с опциональными, платформозависимыми, несовместимыми зависимостями.
    /// Эти зависимости приводят к <see cref="ReflectionTypeLoadException"/>,
    /// мы его перехватываем и достаём из него не-null типы.
    /// </summary>
    /// <param name="a">Сборка, из которой достаём типы</param>
    /// <returns>Загружаемые типы сборки.</returns>
    private static IEnumerable<Type> SafeGetTypes(Assembly a)
    {
        try { return a.GetTypes(); }
        catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t is not null)!; }
    }
}