using FinanceTrack.Gateway.Configuration;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Yarp
builder
    .Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Yarp. Proxy all routes in configuration
app.MapReverseProxy();

await app.RunAsync();
