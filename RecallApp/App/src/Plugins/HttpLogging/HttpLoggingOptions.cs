namespace HttpLogging;

/// <summary>
/// Настройки плагина HTTP-логирования.
/// </summary>
public sealed class HttpLoggingOptions
{
    /// <summary>
    /// Возвращает или задает имя сервиса, записываемое в колонку service_name.
    /// </summary>
    /// <example>
    /// app, bff, ...
    /// </example>
    public string ServiceName { get; set; } = "app";

    /// <summary>
    /// Возвращает или задает схему БД, в которой лежит таблица логов.
    /// </summary>
    /// <remarks>
    /// Схема зафиксирована в сгенерированных миграциях: для её смены требуется пересоздать миграции.
    /// </remarks>
    public string Schema { get; set; } = "logging";

    /// <summary>
    /// Возвращает или задает размер пула соединений, используемого NLog для записи логов.
    /// </summary>
    /// <remarks>
    /// NLog открывает собственные соединения, поэтому его пул не входит в общий
    /// <c>ConnectionPoolBudget</c> и ограничивается этим значением отдельно.
    /// </remarks>
    public int PoolQuota { get; set; } = 5;

    /// <summary>
    /// Возвращает или задает строку подключения для записи логов через NLog.
    /// </summary>
    /// <remarks>
    /// Задаётся плагином: <see cref="Npgsql.NpgsqlDataSource.ConnectionString"/> не содержит пароль,
    /// поэтому строка передаётся в <see cref="HttpLoggingModule.AddHttpLogging"/> явно.
    /// Если значение не задано, используется строка из DbContext.
    /// </remarks>
    internal string? ConnectionString { get; set; }
}
