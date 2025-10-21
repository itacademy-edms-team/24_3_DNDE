// Identity.Application/Commands/SignUpUser/Response/SignUpUserResponse.cs
using Microsoft.AspNetCore.Mvc;

namespace Identity.Application.Commands.SignUpUser.Response
{
    public class SignUpUserResponse : Commands.Response
    {
        public bool IsSuccess { get; set; }
        public string? UserId { get; set; } // For success
        public ProblemDetails? Error { get; set; } // For errors
    }
}
