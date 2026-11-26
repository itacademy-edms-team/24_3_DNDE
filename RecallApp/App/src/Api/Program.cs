using Accounts.Infrastructure;
using Api;
using Common.Application.Provisioning;
using Common.Infrastructure.Data;
using HttpLogging;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;
using Npgsql;
using Sets.Infrastructure;
using Media.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Remove default Microsoft logging providers
builder.Logging.ClearProviders();
// Let NLog control logging
builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
// Register NLog (nlog.config)
builder.Host.UseNLog();
var bootstrapLogger = LogManager.GetCurrentClassLogger();

try
{
    bootstrapLogger.Info("Starting Web Host...");
    
    var connectionString = builder.Configuration.GetConnectionString("Database")
        ?? throw new InvalidOperationException("Set ConnectionStrings__Database. See docs.");

    builder.Services.AddProblemDetails(options =>
        options.CustomizeProblemDetails = ctx =>
            ctx.ProblemDetails.Extensions["source"] = ProblemSource.App
    );
    builder.Services.AddSingleton(TimeProvider.System);

    // Чтобы изменить общий максимальный пул соединений к БД, который распределяют все модули, явно задайте Max Pool Size к ConnectionStrings__Database.
    // Сейчас используется дефолт в 100, заданный NpgsqlConnectionStringBuilder, т.к. в строке подключения явно Max Pool Size не задан.
    var dataSource = new NpgsqlDataSourceBuilder(connectionString).Build();
    var poolBudget = new ConnectionPoolBudget(dataSource);
    var accountsPoolLease = poolBudget.GetPools(
        builder.Configuration.GetValue("AppDb:AccountsPoolQuota", 20));
    var setsPoolLease = poolBudget.GetPools(
        builder.Configuration.GetValue("AppDb:SetsPoolQuota", 20));
    var mediaPoolLease = poolBudget.GetPools(
        builder.Configuration.GetValue("AppDb:MediaPoolQuota", 10));
    builder.Services.AddSingleton(dataSource);
    builder.Services.AddSingleton(poolBudget);
    builder.Services.AddSingleton(new PoolLeaseHolder(accountsPoolLease));
    builder.Services.AddSingleton(new PoolLeaseHolder(setsPoolLease));
    builder.Services.AddSingleton(new PoolLeaseHolder(mediaPoolLease));

    // Идентичность доменного пользователя для модулей: заполняется ступенью JIT-провижининга
    builder.Services.AddScoped<CurrentUser>();
    builder.Services.AddScoped<ICurrentUser>(services => services.GetRequiredService<CurrentUser>());

    builder.Services.AddAccounts(dataSource);
    builder.Services.AddSets(dataSource);
    builder.Services.AddMedia(dataSource);

    // Логирование: DbContext для миграций и источник подключения для NLog DatabaseTarget.
    var loggingConnectionString = builder.Configuration.GetConnectionString("Logging")
        ?? throw new InvalidOperationException("Set ConnectionStrings__Logging (app_runtime DML). See docs.");
    builder.Services.AddDbContext<LoggingDbContext>(options => options.UseNpgsql(loggingConnectionString,
        pg => pg.MigrationsHistoryTable("__EFMigrationsHistory", "logging")));
    builder.Services.AddHttpDatabaseLogging<LoggingDbContext>("app");

    builder.Services.AddAuthentication()
        .AddJwtBearer(options =>
        {
            var authority = builder.Configuration["Oidc:Authority"]
                ?? throw new InvalidOperationException("Set Oidc__Authority (Keycloak realm URL).");
            var audience = builder.Configuration["Oidc:Audience"]
                ?? throw new InvalidOperationException("Set Oidc__Audience (Keycloak client ID issued for this API).");
            options.Authority = authority;
            options.RequireHttpsMetadata = builder.Configuration.GetValue("Oidc:RequireHttpsMetadata", !builder.Environment.IsDevelopment());
            options.MapInboundClaims = false; // сырые имена claim'ов (sub, email, ...), без маппинга в ClaimTypes
            options.TokenValidationParameters.ValidateAudience = true;
            options.TokenValidationParameters.ValidAudience = audience;
            options.TokenValidationParameters.NameClaimType = "name";
            options.TokenValidationParameters.RoleClaimType = "roles";
        });
    builder.Services.AddAuthorization();

    var app = builder.Build();

    app.UseExceptionHandler();
    app.UseHttpDatabaseLogging();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseDomainProvisioning();

    app.MapAccounts();
    app.MapSets();
    app.MapMedia();
    app.MapGet("/health/live", () => Results.Ok(new { Status = "alive" }));
    app.MapGet("/health/ready", async (IServiceProvider services, CancellationToken cancellationToken) =>
    {
        var accounts = services.CanConnectAccounts(cancellationToken);
        var sets = services.CanConnectSets(cancellationToken);
        var media = services.CanConnectMedia(cancellationToken);
        await Task.WhenAll(accounts, sets, media);

        return accounts.Result && sets.Result && media.Result
            ? Results.Ok(new { Status = "ready" })
            : Results.StatusCode(503);
    });

    var migrateOnly = args.Contains("--migrate");
    if (migrateOnly || builder.Configuration.GetValue("Database:ApplyMigrations", false))
    {
        // Миграции выполняются под пользователем, который привязан к конкретной схеме и модулю.
        var migrationsConnectionString = builder.Configuration.GetConnectionString("AccountsMigrations")
            ?? throw new InvalidOperationException("Set ConnectionStrings__AccountsMigrations (schema owner).");
        await AccountsModule.MigrateAccounts(migrationsConnectionString);
        var setsMigrationsConnectionString = builder.Configuration.GetConnectionString("SetsMigrations")
            ?? throw new InvalidOperationException("Set ConnectionStrings__SetsMigrations (schema owner).");
        await SetsModule.MigrateSets(setsMigrationsConnectionString);
        var mediaMigrationsConnectionString = builder.Configuration.GetConnectionString("MediaMigrations")
            ?? throw new InvalidOperationException("Set ConnectionStrings__MediaMigrations (schema owner).");
        await MediaModule.MigrateMedia(mediaMigrationsConnectionString);
        var loggingMigrationsConnectionString = builder.Configuration.GetConnectionString("LoggingMigrations")
            ?? throw new InvalidOperationException("Set ConnectionStrings__LoggingMigrations (logging schema owner).");
        await LoggingModule.MigrateLogging(loggingMigrationsConnectionString);
        if (migrateOnly) return;
    }

    await app.RunAsync();
}
// HostAbortedException бросает WebApplicationFactory, когда забирает собранный хост в тестах.
catch (Exception ex) when (ex is not HostAbortedException)
{
    bootstrapLogger.Error(ex, "Application startup failed");
}
finally
{
    LogManager.Shutdown();
}

public partial class Program;
