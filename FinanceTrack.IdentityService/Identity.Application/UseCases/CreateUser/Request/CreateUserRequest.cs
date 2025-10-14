using System.Collections.Generic;

namespace Identity.Application.UseCases.CreateUser.Request
{
    public class CreateUserRequest : UseCases.Request
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordConfirm { get; set; }
    }
}
