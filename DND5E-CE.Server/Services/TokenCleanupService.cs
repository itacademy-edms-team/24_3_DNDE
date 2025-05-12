using DND5E_CE.Server.Data;

namespace DND5E_CE.Server.Services
{
    public class TokenCleanupService: IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private Timer _timer;

        public TokenCleanupService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Launch timer to clean up used and expired tokens every 24 hours
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromDays(1));
            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DND5EContext>();

                // Delete expired tokens
                var expiredTokens = dbContext.RefreshTokens.Where(rt => rt.ExpiryDate < DateTime.UtcNow);
                dbContext.RefreshTokens.RemoveRange(expiredTokens);

                // Delete used tokens older than 7 days
                var oldUsedTokens = dbContext.RefreshTokens.Where(rt =>
                    rt.IsUsed && rt.AddedDate < DateTime.UtcNow.AddDays(-7));
                dbContext.RefreshTokens.RemoveRange(oldUsedTokens);

                // Delete revoked tokens older than 7 days
                var oldRevokedTokens = dbContext.RefreshTokens.Where(rt =>
                    rt.IsRevoked && rt.AddedDate < DateTime.UtcNow.AddDays(-7));
                dbContext.RefreshTokens.RemoveRange(oldRevokedTokens);

                await dbContext.SaveChangesAsync();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

    }
}
