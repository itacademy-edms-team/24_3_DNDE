using Npgsql;

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

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Check DB connection
            using (var scope = app.Services.CreateScope())
            {
                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                string? connectionString = configuration.GetConnectionString("DefaultConnection");
                if (connectionString == null) throw new Exception("DefaultConnection to DB is not set in Secrets or Environment");

                app.Logger.LogInformation("Checking connection: {ConnectionString}", connectionString);

                try
                {
                    using var connection = new NpgsqlConnection(connectionString);
                    connection.Open();
                    app.Logger.LogInformation("Connection success!");
                }
                catch (Exception ex)
                {
                    app.Logger.LogError(ex, "Connection error.");
                }
            }

            app.UseDefaultFiles();
            app.MapStaticAssets();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}
