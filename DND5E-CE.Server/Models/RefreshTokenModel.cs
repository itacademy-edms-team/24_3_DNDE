using Microsoft.AspNetCore.Identity;

namespace DND5E_CE.Server.Models
{
    public class RefreshTokenModel
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public string JwtId { get; set; } = string.Empty;
        public bool IsUsed { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime AddedDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string UserId { get; set; } = string.Empty;

        // Navigational property
        public IdentityUser User { get; init; } = null!;
    }
}
