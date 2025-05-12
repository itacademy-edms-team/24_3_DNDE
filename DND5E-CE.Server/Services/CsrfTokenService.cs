using DND5E_CE.Server.Data;
using DND5E_CE.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace DND5E_CE.Server.Services
{
    public interface ICsrfTokenService
    {
        Task<string> GenerateCsrfTokenAsync(string userId);
        Task<bool> ValidateCsrfTokenAsync(string token, string userId);
        Task CleanExpiredTokensAsync();
    }

    public class CsrfTokenService : ICsrfTokenService
    {
        private readonly DND5EContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CsrfTokenService> _logger;

        public CsrfTokenService(
            DND5EContext context,
            IConfiguration configuration,
            ILogger<CsrfTokenService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GenerateCsrfTokenAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID is null or empty. Cannot generate CSRF token.");
                throw new ArgumentNullException(nameof(userId));
            }

            // Delete existing token
            var existingToken = await _context.CsrfTokens.FirstOrDefaultAsync(t => t.UserId == userId);
            if (existingToken != null)
            {
                _context.CsrfTokens.Remove(existingToken);
                await _context.SaveChangesAsync();
                _logger.LogDebug("Removed existing CSRF token for user with id: {UserId}",
                    userId);
            }

            // Create new token and add it to DB
            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            var csrfToken = new CsrfTokenModel
            {
                Token = token,
                UserId = userId,
                IssuedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["Csrf:TokenExpiryInDays"]))
            };
            _context.CsrfTokens.Add(csrfToken);
            await _context.SaveChangesAsync();

            _logger.LogInformation("CSRF token generated for user: {UserId}", userId);
            return token;
        }

        public async Task<bool> ValidateCsrfTokenAsync(string token, string userId)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("CSRF validation failed: token or userId is empty");
                return false;
            }

            var csrfToken = await _context.CsrfTokens
                .FirstOrDefaultAsync(t => t.Token == token && t.UserId == userId);

            if (csrfToken == null)
            {
                _logger.LogWarning("CSRF token not found for user: {UserId}", userId);
                return false;
            }

            if (csrfToken.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("CSRF token expired for user: {UserId}", userId);
                _context.CsrfTokens.Remove(csrfToken);
                await _context.SaveChangesAsync();
                return false;
            }

            _logger.LogDebug("CSRF token validated for user: {UserId}", userId);
            return true;
        }

        public async Task CleanExpiredTokensAsync()
        {
            var expiredTokens = await _context.CsrfTokens
                .Where(t => t.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();
            if (expiredTokens.Any())
            {
                _context.CsrfTokens.RemoveRange(expiredTokens);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Removed {Count} expired CSRF tokens", expiredTokens.Count);
            }
        }
    }
}
