using System.ComponentModel.DataAnnotations;

namespace DND5E_CE.Server.Models
{
    public class TokenRequest
    {
        [Required]
        public string AccessToken { get; set; }
        [Required]
        public string RefreshToken { get; set; }

    }
}
