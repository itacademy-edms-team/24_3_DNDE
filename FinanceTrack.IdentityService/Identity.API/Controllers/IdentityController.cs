using Idenitity.Infrastructure.Services.Jwt;
using Identity.Application.Commands.SignInUser;
using Identity.Application.Commands.SignInUser.Request;
using Identity.Application.Commands.SignUpUser;
using Identity.Application.Commands.SignUpUser.Request;
using Identity.Application.Commands.SignUpUser.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Auth.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController(
        IOptions<JwtOptions> jwtOptions,
        ISignUpUserCommand signUpCommand,
        ISignInUserCommand signInCommand
    ) : ControllerBase
    {
        [AllowAnonymous]
        [HttpPost]
        [Route("SignUp")]
        public async Task<IActionResult> SignUpUser(SignUpUserRequest request)
        {
            var response = await signUpCommand.Execute(request);

            if (response.IsSuccess)
            {
                return CreatedAtAction(
                    nameof(SignUpUser),
                    new { userId = response.UserId },
                    new { response.UserId }
                );
            }

            return Problem(
                statusCode: response.Error.Status,
                title: response.Error.Title,
                detail: response.Error.Detail
            );
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("SignIn")]
        public async Task<IActionResult> SignInUser(SignInUserRequest request)
        {
            var response = await signInCommand.Execute(request);

            if (response.IsSuccess)
            {
                var dtNow = DateTime.Now;

                var accesTokenExpires = dtNow.AddMinutes(
                    jwtOptions.Value.AccessTokenOptions.LifeTimeInMinutes
                );
                HttpContext.Response.Cookies.Append(
                    "access_token",
                    response.AccessToken,
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = accesTokenExpires,
                    }
                );

                var refreshTokenExpires = dtNow.AddMinutes(
                    jwtOptions.Value.RefreshTokenOptions.LifeTimeInMinutes
                );
                HttpContext.Response.Cookies.Append(
                    "refresh_token",
                    response.RefreshToken,
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = refreshTokenExpires, // Match database Expires
                    }
                );

                return Ok();
            }

            return Problem(
                statusCode: response.Error.Status,
                title: response.Error.Title,
                detail: response.Error.Detail
            );
        }
    }
}
