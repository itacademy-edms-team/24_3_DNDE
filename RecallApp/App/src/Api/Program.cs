using Accounts.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Database")
    ?? throw new InvalidOperationException("Set ConnectionStrings__Database. See docs.");

builder.Services.AddProblemDetails();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddAccounts(connectionString);

builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        var authority = builder.Configuration["Oidc:Authority"]
            ?? throw new InvalidOperationException("Set Oidc__Authority (Keycloak realm URL).");
        options.Authority = authority;
        options.RequireHttpsMetadata = builder.Configuration.GetValue("Oidc:RequireHttpsMetadata", !builder.Environment.IsDevelopment());
        options.TokenValidationParameters.ValidateAudience = false; // ADR 0028: aud = account in Keycloak
        options.TokenValidationParameters.NameClaimType = "name";
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

app.MapAccounts();
app.MapGet("/health/live", () => Results.Ok(new { Status = "alive" }));
app.MapGet("/health/ready", async (IServiceProvider services, CancellationToken cancellationToken) =>
    await services.CanConnectAccounts(cancellationToken)
        ? Results.Ok(new { Status = "ready" })
        : Results.StatusCode(503));

var migrateOnly = args.Contains("--migrate");
if (migrateOnly || builder.Configuration.GetValue("Database:ApplyMigrations", false))
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.MigrateAccounts();
    if (migrateOnly) return;
}

await app.RunAsync();

public partial class Program;
