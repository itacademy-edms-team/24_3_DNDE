using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Application.Commands.RefreshTokens.Response
{
    public class RefreshTokensResponse : Commands.Response
    {
        public bool IsSuccess { get; set; }
        public string? AccessToken { get; set; } // For success
        public string? RefreshToken { get; set; } // For success
        public ProblemDetails? Error { get; set; } // For errors
    }
}
