namespace EDMS1.CommandLog.IntegrationTests.Helpers;

/// <summary>
/// Хелпер для создания уникальных имён БД, участвующих в тестовых сценариях
/// </summary>
public static class DbNameCreator
{
    /// <summary>
    /// Генерирует имя для базы данных с применением Guid.
    /// </summary>
    /// <returns>Строку вида "test-abcd-efgh-ij...", где "abcd-efgh-ij..." - <see cref="Guid"/>.</returns>
    public static string CreateDbName()
    {
        return $"test-{Guid.NewGuid()}";
    }
}