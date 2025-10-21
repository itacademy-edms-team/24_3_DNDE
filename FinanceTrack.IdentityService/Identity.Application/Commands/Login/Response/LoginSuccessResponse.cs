namespace Identity.Application.Commands.Login.Response
{
    public class LoginSuccessResponse : LoginResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
