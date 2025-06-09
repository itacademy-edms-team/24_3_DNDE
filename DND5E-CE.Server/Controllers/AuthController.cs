using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DND5E_CE.Server.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Authorization;
using DND5E_CE.Server.Services;
using Microsoft.EntityFrameworkCore;
using DND5E_CE.Server.Data;
using DND5E_CE.Server.Models.Auth;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Web;
using DND5E_CE.Server.DTO.Auth;

namespace DND5E_CE.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DND5EContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly Services.IEmailSender _emailSender;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            DND5EContext context,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ITokenService tokenService,
            Services.IEmailSender emailSender,
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _context = context;
            _userManager = userManager;
            _tokenService = tokenService;
            _emailSender = emailSender;
            _configuration = configuration;
            _logger = logger;
        }

        private void setAuthCookies(TokenResponse tokens)
        {
            // access_token
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None, // None is set for cross-domain requests
                Expires = DateTimeOffset.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:AccesTokenExpiryTimeInMinutes"])),
                Path = "/",
                Secure = true, // true is required for cross-domain requests
                IsEssential = true
            };
            Response.Cookies.Append("access_token", tokens.AccessToken, cookieOptions);

            // refresh_token
            cookieOptions.Expires = DateTimeOffset.UtcNow.AddDays(7);
            Response.Cookies.Append("refresh_token", tokens.RefreshToken, cookieOptions);

            _logger.LogInformation("Cookies set: access_token, refresh_token");
        }

        private void clearAuthCookies()
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true,
                Path = "/"
            };
            Response.Cookies.Delete("access_token", cookieOptions);
            Response.Cookies.Delete("refresh_token", cookieOptions);
            _logger.LogInformation("Auth cookies cleared: access_token, refresh_token");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            // Check model validity
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid registration data for user: {Email}",
                    model.Email);
                return BadRequest(new AuthResponse()
                {
                    Success = false,
                    Errors = new[] { "Некорректные регистрационные данные" }
                });
            }

            // Create new user
            var user = new IdentityUser { UserName=model.Username, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                _logger.LogWarning("User {Email} registration failed: {Errors}",
                    model.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                return BadRequest(result.Errors);
            }

            // ON RELEASE: Complete email verification
            //// Email confirmation token generation
            //var frontUrl = _configuration.GetSection("Cors").GetValue<string>("origins");
            //var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            //var callbackUrl = $"{frontUrl}/api/auth/confirm-email?userId={user.Id}&token={HttpUtility.UrlEncode(token)}";

            //// Sending email
            //var emailMessage = $"Please confirm your email by clicking <a href='{callbackUrl}'>here</a>. If it wasn't you, ignore it.";
            //await _emailSender.SendEmailAsync(model.Email, "Confirm Your Email", emailMessage);

            _logger.LogInformation("User {Email} registered successfully",
                model.Email);
            return Created();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            // Login attempts limited in Program.cs

            // Check model validity
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid login data for user: {Email}",
                    model.Email);
                return BadRequest(new AuthResponse()
                {
                    Success = false,
                    Errors = new[] { "Некорректные данные входа" }
                });
            }

            // Check if user exists
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser == null)
            {
                _logger.LogWarning("User {Email} not found",
                    model.Email);
                return BadRequest(new AuthResponse()
                {
                    Success = false,
                    Errors = new[] { "Неверная почта или пароль" }
                });
            }

            // No need to check user exist by UserName (One Email to Many Usernames case)
            // because Identity already configured to prevent that

            // Check password
            var isPasswordCorrect = await _userManager.CheckPasswordAsync(existingUser, model.Password);
            if (!isPasswordCorrect)
            {
                _logger.LogWarning("Incorrect password for user: {Email}",
                    model.Email);
                return BadRequest(new AuthResponse()
                {
                    Success = false,
                    Errors = new[] { "Неверная почта или пароль" }
                });
            }
            
            // Generate tokens
            var tokens = await _tokenService.GenerateTokensAsync(existingUser);

            // Add tokens to cookies
            setAuthCookies(tokens);

            return Ok(new AuthResponse() { Success = true });
        }

        [HttpPost("update-tokens")]
        public async Task<IActionResult> UpdateTokens()
        {
            // Get refresh token from cookie
            var refreshToken = Request.Cookies["refresh_token"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                clearAuthCookies();
                _logger.LogWarning("Refresh token is not provided.");
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Errors = new[] { "Токен обновления не предоставлен" }
                });
            }

            try
            {
                var tokens = await _tokenService.RefreshTokensAsync(refreshToken);
                setAuthCookies(tokens);
                _logger.LogInformation("Tokens updated successfully for user");
                return Ok(new AuthResponse { Success = true });
            }
            catch (SecurityTokenException ex)
            {
                clearAuthCookies();
                _logger.LogWarning("Token refresh failed: {Message}", ex.Message);
                return Unauthorized(new AuthResponse
                {
                    Success = false,
                    Errors = new[] { ex.Message }
                });
            }
        }

        [Authorize]
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken()
        {
            var userId = User.FindFirst("id")?.Value;
            var refreshToken = Request.Cookies["refresh_token"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogWarning("Refresh token not provided in cookie for revoke request, UserId: {UserId}, IP: {ClientIp}",
                    userId, HttpContext.Connection.RemoteIpAddress?.ToString());
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Errors = new[] { "Токен обновления не предоставлен" }
                });
            }

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in claims, IP: {ClientIp}",
                    HttpContext.Connection.RemoteIpAddress?.ToString());
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Errors = new[] { "Пользователь не аутентифицирован" }
                });
            }

            var result = await _tokenService.RevokeTokenAsync(refreshToken, userId);
            if (!result)
            {
                _logger.LogWarning("Failed to revoke token for user: {UserId}, IP: {ClientIp}",
                    userId, HttpContext.Connection.RemoteIpAddress?.ToString());
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Errors = new[] { "Не удалось отозвать токен" }
                });
            }

            clearAuthCookies();
            _logger.LogInformation("Token revoked successfully for user: {UserId}, IP: {ClientIp}",
                userId, HttpContext.Connection.RemoteIpAddress?.ToString());
            return Ok(new AuthResponse { Success = true });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] PasswordChangeDto dto)
        {
            if (!ModelState.IsValid || dto.NewPassword != dto.NewPasswordConfirm)
            {
                _logger.LogWarning("Invalid change password attempt for user");
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Errors = new[] { "Некорректные данные или пароли не совпадают" }
                });
            }

            //var claims = User.Claims.Select(c => $"{c.Type}: {c.Value}");
            //_logger.LogInformation("User claims: {Claims}, IP: {ClientIp}",
            //    string.Join("; ", claims), HttpContext.Connection.RemoteIpAddress?.ToString());

            var userId = User.FindFirst("id")?.Value;
            if (userId == null)
            {
                _logger.LogWarning("User not found for change password");
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Errors = new[] { "Id пользователя не предоставлен" }
                });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found for change password");
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Errors = new[] { "Пользователь не найден" }
                });
            }

            var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, dto.OldPassword);
            if (!isPasswordCorrect)
            {
                _logger.LogWarning("Incorrect old password for user: {Email}",
                    user.Email);
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Errors = new[] { "Неверный текущий пароль" }
                });
            }

            var result = await _userManager.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Password change failed for user: {Email}, Errors: {Errors}, IP: {ClientIp}",
                    user.Email, string.Join(", ", result.Errors.Select(e => e.Description)),
                    HttpContext.Connection.RemoteIpAddress?.ToString());
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Errors = result.Errors.Select(e => e.Description).ToArray()
                });
            }

            // Remove all Refresh-tokens for user
            var refreshTokens = await _context.RefreshTokens.Where(rt => rt.UserId == user.Id).ToListAsync();
            _context.RefreshTokens.RemoveRange(refreshTokens);
            await _context.SaveChangesAsync();

            clearAuthCookies();

            _logger.LogInformation("Password changed successfully for user: {Email}", user.Email);
            return Ok(new AuthResponse()
            {
                Success = true
            });
        }

        // ON RELEASE: Add ForgotPassword Controller here

        [HttpGet("check-auth")]
        [Authorize]
        public IActionResult CheckAuth()
        {
            _logger.LogInformation("Claims: {Claims}", string.Join(", ", User.Claims.Select(c => $"{c.Type}: {c.Value}")));

            return Ok(new AuthResponse
            {
                Success = true
            });
        }
    }
}
