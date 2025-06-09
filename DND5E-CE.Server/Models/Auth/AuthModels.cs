using System.ComponentModel.DataAnnotations;

namespace DND5E_CE.Server.Models.Auth
{
    public class RegisterModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class LoginModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class AuthResponse
    {
        public bool Success { get; set; }
        public IReadOnlyList<string> Errors { get; init; }
    }
}
