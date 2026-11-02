using Accounts.Infrastructure;
using Api;
using Common.Infrastructure.Data;
using NLog;
using NLog.Web;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Remove default Microsoft logging providers
builder.Logging.ClearProviders();
// Register NLog
builder.Host.UseNLog();
var bootstrapLogger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    bootstrapLogger.Info("Starting Web Host...");
    
    var connectionString = builder.Configuration.GetConnectionString("Database")
        ?? throw new InvalidOperationException("Set ConnectionStrings__Database. See docs.");

    builder.Services.AddProblemDetails();
    builder.Services.AddSingleton(TimeProvider.System);

    // Чтобы изменить общий максимальный пул соединений к БД, который распределяют все модули, явно задайте Max Pool Size к ConnectionStrings__Database.
    // Сейчас используется дефолт в 100, заданный NpgsqlConnectionStringBuilder, т.к. в строке подключения явно Max Pool Size не задан.
    var dataSource = new NpgsqlDataSourceBuilder(connectionString).Build();
    var poolBudget = new ConnectionPoolBudget(dataSource);
    var accountsPoolLease = poolBudget.GetPools(
        builder.Configuration.GetValue("AppDb:AccountsPoolQuota", 20));
    builder.Services.AddSingleton(dataSource);
    builder.Services.AddSingleton(poolBudget);
    builder.Services.AddSingleton(new PoolLeaseHolder(accountsPoolLease));

    builder.Services.AddAccounts(dataSource);

    builder.Services.AddAuthentication()
        .AddJwtBearer(options =>
        {
            var authority = builder.Configuration["Oidc:Authority"]
                ?? throw new InvalidOperationException("Set Oidc__Authority (Keycloak realm URL).");
            options.Authority = authority;
            options.RequireHttpsMetadata = builder.Configuration.GetValue("Oidc:RequireHttpsMetadata", !builder.Environment.IsDevelopment());
            options.MapInboundClaims = false; // сырые имена claim'ов (sub, email, ...), без маппинга в ClaimTypes
            options.TokenValidationParameters.ValidateAudience = false; // ADR 0028: aud = account in Keycloak
            options.TokenValidationParameters.NameClaimType = "name";
        });
    builder.Services.AddAuthorization();

    var app = builder.Build();

    app.UseExceptionHandler();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseDomainProvisioning();

    app.MapAccounts();
    app.MapGet("/health/live", () => Results.Ok(new { Status = "alive" }));
    app.MapGet("/health/ready", async (IServiceProvider services, CancellationToken cancellationToken) =>
        await services.CanConnectAccounts(cancellationToken)
            ? Results.Ok(new { Status = "ready" })
            : Results.StatusCode(503));

    var migrateOnly = args.Contains("--migrate");
    if (migrateOnly || builder.Configuration.GetValue("Database:ApplyMigrations", false))
    {
        // Миграции выполняются под пользователем, который привязан к конкретной схеме и модулю.
        var migrationsConnectionString = builder.Configuration.GetConnectionString("AccountsMigrations")
            ?? throw new InvalidOperationException("Set ConnectionStrings__AccountsMigrations (schema owner).");
        await AccountsModule.MigrateAccounts(migrationsConnectionString);
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
