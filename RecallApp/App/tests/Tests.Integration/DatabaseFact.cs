namespace Tests.Integration;

/// <summary>
/// Fact для тестов, требующих живую app_db.
/// Атрибут сам решает выполнять или пропускать: без строки подключения миграций тест пропускается (Skip),
/// так Run All локально не падает, а в контейнере тест выполняется.
/// Фильтрация по желанию: [Trait("Category", "Database")].
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class DatabaseFactAttribute : FactAttribute
{
    public DatabaseFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("ConnectionStrings__AccountsMigrations") is null)
            Skip = "Требует живую app_db: docker compose --profile test run --rm app-tests.";
    }
}
