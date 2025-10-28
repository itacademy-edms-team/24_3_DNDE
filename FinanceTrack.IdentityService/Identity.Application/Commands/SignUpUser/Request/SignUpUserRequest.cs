using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Identity.Application.Commands.SignUpUser.Request
{
    public class SignUpUserRequest : Commands.Request
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "PasswordConfirm is required.")]
        public string PasswordConfirm { get; set; } = null!;
    }
}
