using System.Text;
using Idenitity.Domain;
using Idenitity.Infrastructure.Services.Jwt;
using Identity.API.Startup;
using Identity.Application.Commands.SignInUser;
using Identity.Application.Commands.SignUpUser;
using Identity.Application.Ports.Repositories;
using Identity.Application.Ports.Services;
using Identity.Domain;
using Identity.Infrastucture.Data;
using Identity.Infrastucture.Repositories.PostgreSQL;
using Identity.Infrastucture.Repositories.Redis;
using Identity.Infrastucture.Services.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.Configure<PostgreSqlOptions>(
    builder.Configuration.GetSection("PostgreSqlOptions")
);
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JwtOptions"));
builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection("RedisOptions"));

// PostgreSQL
var postgreSqlSettings = builder
    .Configuration.GetSection("PostgreSqlOptions")
    .Get<PostgreSqlOptions>();
builder.Services.AddDbContext<AppIdentityContext>(options =>
    options.UseNpgsql(postgreSqlSettings.ConnectionString)
);

// Add Identity services
builder
    .Services.AddIdentity<User, Role>()
    .AddEntityFrameworkStores<AppIdentityContext>()
    .AddDefaultTokenProviders();

// Repositories
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// Authentication
builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("JwtOptions").Get<JwtOptions>();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.AccessTokenOptions.Issuer,
            ValidAudience = jwtSettings.AccessTokenOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.AccessTokenOptions.SigningKey)
            ),
        };
        // Read access_token from HttpOnly cookie
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies["access_token"];
                return Task.CompletedTask;
            },
        };
    })
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

// Roles
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SignInRequired", policy => policy.RequireRole("User"));
});

// JwtService
builder.Services.AddScoped<IAuthTokenService, JwtService>();

// PasswordCheckService
builder.Services.AddScoped<IUserPasswordSignInService, UserPasswordSignInService>();

// Commands
builder.Services.AddScoped<ISignUpUserCommand, SignUpUserCommand>();
builder.Services.AddScoped<ISignInUserCommand, SignInUserCommand>();

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Identity.API", Version = "v1" });
});

builder.Services.AddLogging();

var app = builder.Build();

// Seeding data
using var scope = app.Services.CreateScope();
var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
try
{
    await IdentityDataSeeder.SeedAsync(app.Services, logger);
    logger.LogInformation("IdentityDataSeeder completed successfully");
}
catch (Exception ex)
{
    logger.LogError(ex, "IdentityDataSeeder failed");
    throw;
}

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity.API v1"));
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
