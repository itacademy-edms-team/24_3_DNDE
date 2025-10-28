using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Identity.Infrastucture.Data
{
    // Used in migrations
    public class AppIdentityContextFactory : IDesignTimeDbContextFactory<AppIdentityContext>
    {
        public AppIdentityContext CreateDbContext(string[] args)
        {
            var currentDir = Directory.GetCurrentDirectory();
            var solutionDir = Directory.GetParent(currentDir)!.FullName;

            var apiDir = Path.Combine(solutionDir, "FinanceTrack.IdentityService");

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.Exists(apiDir) ? apiDir : currentDir)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables();

            var configuration = configBuilder.Build();

            var connectionString = configuration.GetSection("PostgreSqlOptions")[
                "ConnectionString"
            ];

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("PostgreSql connection string is not provided.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<AppIdentityContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new AppIdentityContext(optionsBuilder.Options);
        }
    }
}
