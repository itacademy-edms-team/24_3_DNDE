namespace EDMS1.CommandLog.Extensions;

/// <summary>
/// Класс расширения для типа обопщения.
/// </summary>
public static class GenericTypeExtensions
{
    /// <summary>
    /// Получить обопщенный класс по имени.
    /// </summary>
    public static string GetGenericTypeName(this Type type)
    {
        string typeName;

        if (type.IsGenericType)
        {
            var genericTypes = string.Join(",", type.GetGenericArguments().Select(t => t.Name).ToArray());
            typeName = $"{type.Name.Remove(type.Name.IndexOf('`'))}<{genericTypes}>";
        }
        else
        {
            typeName = type.Name;
        }

        return typeName;
    }

    /// <summary>
    /// Получить обопщенный класс по имени.
    /// </summary>
    public static string GetGenericTypeName(this object @object)
    {
        return @object.GetType().GetGenericTypeName();
    }
}
