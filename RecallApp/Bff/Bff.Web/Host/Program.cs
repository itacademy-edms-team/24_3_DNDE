using Microsoft.AspNetCore.HttpOverrides;
using NLog;
using NLog.Web;

var builder = WebApplication.CreateBuilder(args);

// Remove default Microsoft logging providers
builder.Logging.ClearProviders();
// Register NLog
builder.Host.UseNLog();
var bootstrapLogger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    bootstrapLogger.Info("Starting Web Host...");

    // Configuration
    // Right scheme/host/port forward
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders =
            ForwardedHeaders.XForwardedFor
            | ForwardedHeaders.XForwardedProto
            | ForwardedHeaders.XForwardedHost;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });

    // Services
    builder.Services.AddHealthChecks();

    builder.Services.AddControllers();

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    app.UseDeveloperExceptionPage();
    app.UseForwardedHeaders();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseRouting();

    app.UseAuthorization();

    app.MapHealthChecks("/healthz");
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    bootstrapLogger.Error(ex, "Application startup failed");
}
finally
{
    LogManager.Shutdown();
}
