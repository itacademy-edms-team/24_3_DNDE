using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NLog.Targets.Wrappers;

namespace HttpLogging;

/// <summary>
/// Настраивает NLog DatabaseTarget на основе переданного <typeparamref name="TDbContext"/>.
/// </summary>
internal sealed class NLogDatabaseConfigurator<TDbContext>(
    IServiceProvider serviceProvider,
    HttpLoggingOptions options,
    AccessLogOptions accessLogOptions)
    : IHostedService
    where TDbContext : DbContext
{
    private readonly string _serviceName = options.ServiceName;
    private readonly string _loggerName = accessLogOptions.LoggerName;
    private readonly LogLevel _minLevel = accessLogOptions.MinLevel;
    private readonly int _queueLimit = accessLogOptions.QueueLimit;
    private readonly AsyncTargetWrapperOverflowAction _overflowAction = accessLogOptions.OverflowAction;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

        var connection = dbContext.Database.GetDbConnection();
        var dbProviderName = connection.GetType().AssemblyQualifiedName
                             ?? throw new InvalidOperationException("Unable to determine the DbConnection type from the context.");
        var connectionString = dbContext.Database.GetConnectionString()
                               ?? throw new InvalidOperationException("The DbContext connection string is not configured.");

        var sqlHelper = dbContext.GetService<ISqlGenerationHelper>();

        var entityType = dbContext.Model.FindEntityType(typeof(LogEntry))
            ?? throw new InvalidOperationException(
                $"Entity {nameof(LogEntry)} was not found in {typeof(TDbContext).Name}. Call UseDatabaseLogging() in OnModelCreating.");

        var schema = entityType.GetSchema();
        var tableName = entityType.GetTableName()!;
        var quotedTable = sqlHelper.DelimitIdentifier(tableName, schema);
        var storeObject = StoreObjectIdentifier.Table(tableName, schema);

        var columns = BuildColumns(entityType, storeObject, sqlHelper, _serviceName);
        var commandText = BuildInsert(quotedTable, columns);

        var targetName = $"db_logging_{typeof(TDbContext).Name}";
        var dbTarget = new DatabaseTarget(targetName)
        {
            DBProvider = dbProviderName,
            ConnectionString = connectionString,
            CommandText = commandText,
            KeepConnection = false,
        };

        foreach (var column in columns)
        {
            dbTarget.Parameters.Add(new DatabaseParameterInfo(column.ParameterName, column.Layout)
            {
                DbType = column.DbType,
                AllowDbNull = column.AllowDbNull,
            });
        }

        var asyncWrapper = new AsyncTargetWrapper(dbTarget)
        {
            Name = $"async_{targetName}",
            QueueLimit = _queueLimit,
            OverflowAction = _overflowAction,
        };

        var config = LogManager.Configuration ?? new LoggingConfiguration();
        config.AddTarget(asyncWrapper);

        // Защита от рекурсии: собственные логи EF Core не пишем в БД.
        if (config.FindTargetByName("ef_null") is null)
        {
            var nullTarget = new NullTarget("ef_null");
            config.AddTarget(nullTarget);
            config.AddRule(LogLevel.Trace, LogLevel.Info, nullTarget, "Microsoft.EntityFrameworkCore.*", final: true);
        }

        config.AddRule(_minLevel, LogLevel.Fatal, asyncWrapper, _loggerName);
        LogManager.Configuration = config;

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static List<ColumnSpec> BuildColumns(
        IEntityType entityType,
        StoreObjectIdentifier storeObject,
        ISqlGenerationHelper sqlHelper,
        string serviceName)
    {
        var specs = new List<ColumnSpec>();

        Add(nameof(LogEntry.StartTime), "${http-value:source=StartTime}", nameof(System.Data.DbType.DateTimeOffset), false);
        Add(nameof(LogEntry.Duration), "${http-value:source=DurationMs}", nameof(System.Data.DbType.Double), true);
        Add(nameof(LogEntry.Level), "${level:uppercase=true}", nameof(System.Data.DbType.String), false);
        Add(nameof(LogEntry.Logger), "${logger}", nameof(System.Data.DbType.String), false);
        Add(nameof(LogEntry.Message), "${message}", nameof(System.Data.DbType.String), false);
        Add(nameof(LogEntry.MessageProperties), "${http-message-properties}", nameof(System.Data.DbType.String), true);
        Add(nameof(LogEntry.ExceptionType), "${exception:format=Type}", nameof(System.Data.DbType.String), true);
        Add(nameof(LogEntry.ExceptionMessage), "${exception:format=Message}", nameof(System.Data.DbType.String), true);
        Add(nameof(LogEntry.RootExceptionType), "${http-root-exception:format=Type}", nameof(System.Data.DbType.String), true);
        Add(nameof(LogEntry.RootExceptionMessage), "${http-root-exception:format=Message}", nameof(System.Data.DbType.String), true);
        Add(nameof(LogEntry.StackTraceShort), "${http-text:source=StackTrace:maxLength=4000}", nameof(System.Data.DbType.String), true);
        Add(nameof(LogEntry.StackTraceCompressed), "${http-gzip:source=StackTrace}", nameof(System.Data.DbType.String), true, compressed: true);
        Add(nameof(LogEntry.ClientIp), "${aspnet-request-ip}", nameof(System.Data.DbType.String), true);
        Add(nameof(LogEntry.HttpMethod), "${aspnet-request-method}", nameof(System.Data.DbType.String), true);
        Add(nameof(LogEntry.RequestPath), "${aspnet-request-url:IncludeQueryString=false}", nameof(System.Data.DbType.String), true);
        Add(nameof(LogEntry.RequestQuery), "${http-text:source=RequestQuery}", nameof(System.Data.DbType.String), true);
        Add(nameof(LogEntry.ResponseStatusCode), "${http-value:source=StatusCode}", nameof(System.Data.DbType.Int32), true);
        Add(nameof(LogEntry.RequestHeadersShort), "${http-text:source=RequestHeaders:maxLength=4000}", nameof(System.Data.DbType.String), true);
        Add(nameof(LogEntry.RequestHeadersCompressed), "${http-gzip:source=RequestHeaders}", nameof(System.Data.DbType.String), true, compressed: true);
        Add(nameof(LogEntry.RequestBodyShort), "${http-text:source=RequestBody:maxLength=4000}", nameof(System.Data.DbType.String), true);
        Add(nameof(LogEntry.RequestBodyCompressed), "${http-gzip:source=RequestBody}", nameof(System.Data.DbType.String), true, compressed: true);
        Add(nameof(LogEntry.ResponseHeadersShort), "${http-text:source=ResponseHeaders:maxLength=4000}", nameof(System.Data.DbType.String), true);
        Add(nameof(LogEntry.ResponseHeadersCompressed), "${http-gzip:source=ResponseHeaders}", nameof(System.Data.DbType.String), true, compressed: true);
        Add(nameof(LogEntry.ResponseBodyShort), "${http-text:source=ResponseBody:maxLength=4000}", nameof(System.Data.DbType.String), true);
        Add(nameof(LogEntry.ResponseBodyCompressed), "${http-gzip:source=ResponseBody}", nameof(System.Data.DbType.String), true, compressed: true);
        Add(nameof(LogEntry.TraceId), "${activity:TraceId}", nameof(System.Data.DbType.String), true);
        Add(nameof(LogEntry.ParentSpanId), "${activity:SpanId:Parent=true}", nameof(System.Data.DbType.String), true);
        Add(nameof(LogEntry.SpanId), "${activity:SpanId}", nameof(System.Data.DbType.String), true);
        Add(nameof(LogEntry.SpanKind), "${activity:ActivityKind}", nameof(System.Data.DbType.String), true);
        Add(nameof(LogEntry.ServiceName), serviceName, nameof(System.Data.DbType.String), false);

        return specs;

        void Add(string property, string layout, string dbType, bool allowDbNull, bool compressed = false)
        {
            var columnName = sqlHelper.DelimitIdentifier(ColumnName(entityType, storeObject, property));
            var parameterName = "@" + property;
            // Сжатые колонки (bytea) пишем через decode(@p, 'base64'), т.к. IRawValue (byte[]) закрыт в NLog.
            var sqlValue = compressed ? $"decode({parameterName}, 'base64')" : parameterName;
            specs.Add(new ColumnSpec(columnName, parameterName, sqlValue, new SimpleLayout(layout), dbType, allowDbNull));
        }
    }

    private static string ColumnName(IEntityType entityType, StoreObjectIdentifier storeObject, string propertyName)
        => entityType.FindProperty(propertyName)!.GetColumnName(storeObject)!;

    private static string BuildInsert(string quotedTable, List<ColumnSpec> columns)
        => $"INSERT INTO {quotedTable} ({string.Join(", ", columns.Select(c => c.ColumnName))}) "
         + $"VALUES ({string.Join(", ", columns.Select(c => c.SqlValue))})";

    private sealed record ColumnSpec(
        string ColumnName,
        string ParameterName,
        string SqlValue,
        Layout Layout,
        string DbType,
        bool AllowDbNull);
}
