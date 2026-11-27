using Microsoft.EntityFrameworkCore;

namespace HttpLogging;

/// <summary>
/// Набор расширений для работы с EntityFramework.
/// </summary>
public static class EntityFrameworkExtensions
{
    /// <summary>
    /// Подключает таблицу http_logs в модель переданного DbContext.
    /// </summary>
    /// <remarks>
    /// Вызывается в OnModelCreating переданного DbContext.
    /// </remarks>
    /// <param name="modelBuilder">Строитель модели</param>
    /// <param name="schema">Название схемы; null - схема по умолчанию.</param>
    /// <returns><paramref name="modelBuilder"/> для цепочки вызовов.</returns>
    public static ModelBuilder UseDatabaseLogging(this ModelBuilder modelBuilder, string? schema = null)
    {
        modelBuilder.ApplyConfiguration(new LogEntryConfiguration(schema));
        return modelBuilder;
    }
}
