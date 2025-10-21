using System.Text;
using Idenitity.Domain;
using Idenitity.Infrastructure.Services.Jwt;
using Identity.Application.Commands.CreateUser;
using Identity.Application.Commands.Login;
using Identity.Application.Commands.RefreshToken;
using Identity.Application.Commands.SignOut;
using Identity.Application.Ports.Repositories;
using Identity.Domain;
using Identity.Infrastucture.Data;
using Identity.Infrastucture.Repositories.PostgreSQL;
using Identity.Infrastucture.Repositories.Redis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.Configure<PostgreSqlOptions>(
    builder.Configuration.GetSection("PostgreSqlSettings")
);
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection("RedisSettings"));

// PostgreSQL
var postgreSqlSettings = builder
    .Configuration.GetSection("PostgreSqlSettings")
    .Get<PostgreSqlOptions>();
builder.Services.AddDbContext<AppIdentityContext>(options =>
    options.UseNpgsql(postgreSqlSettings.ConnectionString)
);
builder.Services.AddSingleton<IIdentityRepository, IdentityRepository>();

// Redis
var redisSettings = builder.Configuration.GetSection("RedisSettings").Get<RedisOptions>();

// Add Identity services
builder
    .Services.AddIdentity<User, Role>()
    .AddEntityFrameworkStores<AppIdentityContext>()
    .AddDefaultTokenProviders();

// Authentication
builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.AccessTokenSettings.Issuer,
            ValidAudience = jwtSettings.AccessTokenSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.AccessTokenSettings.SigningKey)
            ),
        };
    })
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

// Use-Cases
builder.Services.AddSingleton<LoginCommand>();
builder.Services.AddSingleton<RefreshTokenCommand>();
builder.Services.AddSingleton<SignOutCommand>();
builder.Services.AddSingleton<CreateUserCommand>();

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Identity.API", Version = "v1" });
});

var app = builder.Build();

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

app.Run();
