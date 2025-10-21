using System.Text;
using Idenitity.Application.UseCases.Login;
using Idenitity.Application.UseCases.SignOut;
using Idenitity.Domain;
using Idenitity.Infrastructure.Services.Jwt;
using Identity.Application.Ports.Repositories;
using Identity.Application.UseCases.CreateUser;
using Identity.Application.UseCases.RefreshToken;
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
builder.Services.Configure<PostgreSqlSettings>(
    builder.Configuration.GetSection("PostgreSqlSettings")
);
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection("RedisSettings"));

// PostgreSQL
var postgreSqlSettings = builder
    .Configuration.GetSection("PostgreSqlSettings")
    .Get<PostgreSqlSettings>();
builder.Services.AddDbContext<AppIdentityContext>(options =>
    options.UseNpgsql(postgreSqlSettings.ConnectionString)
);
builder.Services.AddSingleton<IIdentityRepository, IdentityRepository>();

// Redis
var redisSettings = builder.Configuration.GetSection("RedisSettings").Get<RedisSettings>();

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
builder.Services.AddSingleton<LoginUseCase>();
builder.Services.AddSingleton<RefreshTokenUseCase>();
builder.Services.AddSingleton<SignOutUseCase>();
builder.Services.AddSingleton<CreateUserUseCase>();

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
