using Microsoft.AspNetCore.Mvc;

namespace Identity.Application.Commands.SignInUser.Response
{
    public class SignInUserResponse : Commands.Response
    {
        public bool IsSuccess { get; set; }
        public string? AccessToken { get; set; } // For success
        public string? RefreshToken { get; set; } // For success
        public ProblemDetails? Error { get; set; } // For errors
    }
}
