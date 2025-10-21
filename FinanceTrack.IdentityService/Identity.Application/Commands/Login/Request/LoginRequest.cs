namespace Identity.Application.Commands.Login.Request
{
    public class LoginRequest : Commands.Request
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
