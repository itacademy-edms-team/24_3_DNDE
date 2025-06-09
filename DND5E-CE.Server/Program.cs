using DND5E_CE.Server.Data;
using DND5E_CE.Server.Mapping;
using DND5E_CE.Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace DND5E_CE.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add secrets if development
            if (builder.Environment.IsDevelopment())
            {
                builder.Configuration.AddUserSecrets<Program>();
            }

            // JWT key length check
            var jwtKey = builder.Configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey) || Encoding.UTF8.GetBytes(jwtKey).Length < 32)
            {
                throw new InvalidOperationException("JWT Key must be at least 32 bytes long.");
            }

            // DND5EContext configuration using connection string from secrets
            builder.Services.AddDbContext<DND5EContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Identity configuration
            builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                //options.SignIn.RequireConfirmedEmail = true; // Email confirmation required
                options.User.RequireUniqueEmail = true; // Prevent multiple UserName on same Email
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            })
                .AddEntityFrameworkStores<DND5EContext>()
                .AddDefaultTokenProviders();

            // JWT configuration
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!)),
                ClockSkew = TimeSpan.FromMinutes(5)
            };
            builder.Services.AddSingleton(tokenValidationParameters);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = tokenValidationParameters;
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Cookies["access_token"];
                        if (!string.IsNullOrEmpty(accessToken))
                        {
                            context.Token = accessToken;

                            var logger = context.HttpContext.RequestServices
                                .GetRequiredService<ILogger<Program>>();
                            logger.LogInformation("JWT extracted from cookie: {Length} chars",
                                accessToken.Length);
                        }
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = async context =>
                    {
                        var userManager = context.HttpContext.RequestServices
                            .GetRequiredService<UserManager<IdentityUser>>();
                        var userId = context.Principal?.FindFirst("id")?.Value;

                        if (userId == null || await userManager.FindByIdAsync(userId) == null)
                        {
                            context.Fail("Пользователь не найден");
                        }
                    },
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<Program>>();
                        logger.LogWarning("Authentication failed: {Exception}", context.Exception);

                        // Если ошибка с датами токена, логируем подробнее
                        if (context.Exception is SecurityTokenExpiredException)
                        {
                            logger.LogWarning("Token expired");
                        }
                        else if (context.Exception is SecurityTokenInvalidLifetimeException lifetimeEx)
                        {
                            logger.LogWarning("Token lifetime invalid: {Message}", lifetimeEx.Message);
                        }

                        return Task.CompletedTask;
                    }
                };
            });

            // CORS configuration
            var allowedOrigins = builder.Configuration["Cors:Origins"];
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp", builder =>
                {
                    builder.WithOrigins(allowedOrigins) // For develop
                           .AllowAnyHeader()
                           .AllowAnyMethod()
                           .AllowCredentials(); // For cookie usage
                });
            });

            // AutoMapper configuration
            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<AutoMapperProfile>();
            });

            // Add services to the container.
            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
            builder.Services.AddSingleton<IEmailSender, EmailSender>();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddOpenApi();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<ICharacterService,  CharacterService>();
            builder.Services.AddHostedService<TokenCleanupService>();
            builder.Services.AddLogging(logging =>
            {
                logging.AddConsole();
                logging.AddDebug();
                logging.SetMinimumLevel(LogLevel.Information);
            });

            var app = builder.Build();

            app.UseDefaultFiles();
            app.MapStaticAssets();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowReactApp");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
