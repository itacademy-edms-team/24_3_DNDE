using DND5E_CE.Server.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DND5E_CE.Server.Services
{
    public class TokenCleanupService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TokenCleanupService> _logger;
        private Timer _timer;

        public TokenCleanupService(IServiceProvider serviceProvider, ILogger<TokenCleanupService> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("TokenCleanupService started");
            // Start timer to clean up tokens every 24 hours
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromDays(1));
            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<DND5EContext>();

                // Clean expired refresh tokens
                var expiredRefreshTokens = dbContext.RefreshTokens
                    .Where(rt => rt.ExpiryDate < DateTime.UtcNow)
                    .ToList();
                if (expiredRefreshTokens.Any())
                {
                    dbContext.RefreshTokens.RemoveRange(expiredRefreshTokens);
                    _logger.LogInformation("Removed {Count} expired refresh tokens", expiredRefreshTokens.Count);
                }

                // Clean revoked refresh tokens older than 7 days
                var oldRevokedTokens = dbContext.RefreshTokens
                    .Where(rt => rt.IsRevoked && rt.AddedDate < DateTime.UtcNow.AddDays(-7))
                    .ToList();
                if (oldRevokedTokens.Any())
                {
                dbContext.RefreshTokens.RemoveRange(oldRevokedTokens);
                    _logger.LogInformation("Removed {Count} old revoked refresh tokens", oldRevokedTokens.Count);
                }

                await dbContext.SaveChangesAsync();
                _logger.LogInformation("Token cleanup completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token cleanup");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("TokenCleanupService stopped");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

    }
}
