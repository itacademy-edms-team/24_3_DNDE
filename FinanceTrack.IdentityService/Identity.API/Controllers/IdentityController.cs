using System.Net;
using Idenitity.Infrastructure.Services.Jwt;
using Identity.Application.Commands.RefreshTokens;
using Identity.Application.Commands.RefreshTokens.Request;
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
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class IdentityController(
        IOptions<JwtOptions> jwtOptions,
        ISignUpUserCommand signUpCommand,
        ISignInUserCommand signInCommand,
        IRefreshTokensCommand RefreshTokensCommand
    ) : ControllerBase
    {
        /// <summary>
        /// Register a new user.
        /// </summary>
        /// <remarks>
        /// Creates a new identity user and assigns them the default "User" role.
        ///
        /// Possible failure reasons:
        /// - Email already taken
        /// - Password does not satisfy policy
        /// - Password and PasswordConfirm do not match
        /// </remarks>
        /// <param name="request">User credentials and profile data required to create an account.</param>
        /// <response code="201">User successfully created. Returns the new user's ID.</response>
        /// <response code="400">Validation failed (bad password, invalid email, etc.).</response>
        /// <response code="409">User with the same email already exists.</response>
        /// <response code="500">User was partially created or unexpected server/database error occurred.</response>
        [AllowAnonymous]
        [HttpPost]
        [Route("SignUp")]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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
                statusCode: response.Error.Status ?? StatusCodes.Status500InternalServerError,
                title: response.Error.Title,
                detail: response.Error.Detail
            );
        }

        /// <summary>
        /// Sign in and receive auth cookies.
        /// </summary>
        /// <remarks>
        /// On success:
        /// - Sets `access_token` (HttpOnly, Secure, SameSite=Strict)
        /// - Sets `refresh_token` (HttpOnly, Secure, SameSite=Strict)
        ///
        /// You normally don't read these cookies in JS (access token is HttpOnly).
        /// Just include cookies in subsequent requests to authorized endpoints.
        /// </remarks>
        /// <param name="request">Email / username and password.</param>
        /// <response code="200">Authenticated. Cookies were set.</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="401">Invalid credentials.</response>
        /// <response code="500">Unexpected server error.</response>
        [AllowAnonymous]
        [HttpPost("SignIn")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SignInUser(SignInUserRequest request)
        {
            var response = await signInCommand.Execute(request);

            if (response.IsSuccess)
            {
                var nowUtc = DateTime.UtcNow;

                var accesTokenExpires = nowUtc.AddMinutes(
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

                var refreshTokenExpires = nowUtc.AddMinutes(
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
                statusCode: response.Error.Status ?? StatusCodes.Status500InternalServerError,
                title: response.Error.Title,
                detail: response.Error.Detail
            );
        }

        /// <summary>
        /// Get new token pair (access_token and refresh_token) using refresh_token. Store all in HttpOnly cookie.
        /// </summary>
        /// <param name="request">refresh_token in HttpOnly cookie</param>
        /// <returns>new token pair (access_token and refresh_token) in HttpOnly cookie</returns>
        /// <response code="200">Tokens updated successfully</response>
        /// <response code="401">Token not found, Token is invalid (revoked or expired), Token owner not found</response>
        /// <response code="500">Server errors</response>
        [AllowAnonymous]
        [HttpPost("RefreshTokens")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refresh_token"];
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Unauthorized",
                    detail: "Refresh token is not provided."
                );
            }

            var request = new RefreshTokensRequest { RefreshToken = refreshToken };

            var response = await RefreshTokensCommand.Execute(request);

            if (response.IsSuccess)
            {
                var nowUtc = DateTime.UtcNow;

                var accesTokenExpires = nowUtc.AddMinutes(
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

                var refreshTokenExpires = nowUtc.AddMinutes(
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
                        Expires = refreshTokenExpires,
                    }
                );

                return Ok();
            }

            return Problem(
                statusCode: response.Error.Status ?? StatusCodes.Status500InternalServerError,
                title: response.Error.Title,
                detail: response.Error.Detail
            );
        }
    }
}
