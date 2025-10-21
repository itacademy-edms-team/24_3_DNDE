using System.Collections.Generic;

namespace Identity.Application.Commands.CreateUser.Request
{
    public class CreateUserRequest : Commands.Request
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordConfirm { get; set; }
    }
}
