using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DND5E_CE.Server.Models
{
    public class CsrfTokenModel
    {
        [Key]
        [MaxLength(255)]
        public string Token { get; set; }

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; }

        public IdentityUser User { get; set; }

        public DateTime IssuedAt { get; set; }

        public DateTime ExpiresAt { get; set; }
    }
}
