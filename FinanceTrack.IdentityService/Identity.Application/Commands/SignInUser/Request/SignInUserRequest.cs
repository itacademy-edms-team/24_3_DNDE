namespace Identity.Application.Commands.SignInUser.Request
{
    public class SignInUserRequest : Commands.Request
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
