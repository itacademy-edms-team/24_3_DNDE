using System.Text;
using Auth.Infrastructure.Services.Jwt;
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

namespace Identity.API
{
    public class Startup
    {
        private IWebHostEnvironment CurrentEnvironment { get; set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<PostgreSqlSettings>(Configuration.GetSection("PostgreSqlSettings"));
            services.Configure<JwtSettings>(Configuration.GetSection("JwtSettings"));
            services.Configure<RedisSettings>(Configuration.GetSection("RedisSettings"));

            // PostgreSQL
            var postgreSqlSettings = Configuration
                .GetSection("PostgreSqlSettings")
                .Get<PostgreSqlSettings>();
            services.AddDbContext<AppIdentityContext>(options =>
                options.UseNpgsql(postgreSqlSettings.ConnectionString)
            );
            services.AddSingleton<IIdentityRepository, IdentityRepository>();

            // Redis
            var redisSettings = Configuration.GetSection("RedisSettings").Get<RedisSettings>();

            // Add Identity services
            services
                .AddIdentity<User, Role>()
                .AddEntityFrameworkStores<AppIdentityContext>()
                .AddDefaultTokenProviders();

            // Authentication
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    var jwtSettings = Configuration.GetSection("JwtSettings").Get<JwtSettings>();
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
            services.AddSingleton<LoginUseCase>();
            services.AddSingleton<RefreshTokenUseCase>();
            services.AddSingleton<SignOutUseCase>();
            services.AddSingleton<CreateUserUseCase>();

            // Controllers
            services.AddControllers();

            // Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Identity.API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            CurrentEnvironment = env;

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity.API v1")
                );
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
