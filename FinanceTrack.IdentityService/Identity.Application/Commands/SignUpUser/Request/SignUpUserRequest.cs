using System.Collections.Generic;

namespace Identity.Application.Commands.SignUpUser.Request
{
    public class SignUpUserRequest : Commands.Request
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordConfirm { get; set; }
    }
}
