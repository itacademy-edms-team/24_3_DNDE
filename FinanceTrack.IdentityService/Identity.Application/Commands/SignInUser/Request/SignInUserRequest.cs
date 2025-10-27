using System.ComponentModel.DataAnnotations;

namespace Identity.Application.Commands.SignInUser.Request
{
    public class SignInUserRequest : Commands.Request
    {
        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }
}
