using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DND5E_CE.Server.Models
{
    public class RefreshTokenModel
    {
        [Key]
        public int Id { get; set; }
        
        public string TokenHash { get; set; } = string.Empty; // Store Hash except raw token
        public string Salt { get; set; } = string.Empty;

        public bool IsRevoked { get; set; } = false;

        public DateTime? RevokedAt { get; set; }
        
        public DateTime AddedDate { get; set; }
        
        public DateTime ExpiryDate { get; set; }
        
        public string UserId { get; set; } = string.Empty;

        public IdentityUser User { get; init; } = null!;
    }
}
