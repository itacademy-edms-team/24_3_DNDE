using FinanceTrack.Finance.UseCases.Contributors.Create;
using FinanceTrack.Finance.Web.BackgroundServices;
using FinanceTrack.Finance.Web.Configurations;

var builder = WebApplication.CreateBuilder(args);

var logger = Log.Logger = new LoggerConfiguration().Enrich.FromLogContext().CreateLogger();

logger.Information("Starting web host");

builder.AddLoggerConfigs();

var appLogger = new SerilogLoggerFactory(logger).CreateLogger<Program>();

builder.Services.AddOptionConfigs(builder.Configuration, appLogger, builder);
builder.Services.AddServiceConfigs(appLogger, builder);

builder
    .Services.AddFastEndpoints()
    .SwaggerDocument(o =>
    {
        o.ShortSchemaNames = true;
    })
    .AddCommandMiddleware(c =>
    {
        c.Register(typeof(CommandLogger<,>));
    });

// Background services
builder.Services.AddHostedService<RecurringTransactionBackgroundService>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

await app.UseAppMiddlewareAndSeedDatabase();

await app.RunAsync();

// Make the implicit Program.cs class public, so integration tests can reference the correct assembly for host building
public partial class Program { }
