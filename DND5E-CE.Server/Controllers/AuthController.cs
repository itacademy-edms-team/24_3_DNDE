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

namespace DND5E_CE.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DND5EContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly ICsrfTokenService _csrfTokenService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            DND5EContext context,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ITokenService tokenService,
            ICsrfTokenService csrfTokenService,
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _context = context;
            _userManager = userManager;
            _tokenService = tokenService;
            _csrfTokenService = csrfTokenService;
            _configuration = configuration;
            _logger = logger;
        }

        private void setAuthCookies(TokenResponse tokens, string csrfToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None, // None is set for cross-domain requests
                Expires = DateTimeOffset.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:AccesTokenExpiryTimeInMinutes"])),
                Path = "/",
                Secure = true, // false is set for develop. Set to true for production
                IsEssential = true
            };
            Response.Cookies.Append("access_token", tokens.AccessToken, cookieOptions);

            cookieOptions.Expires = DateTimeOffset.UtcNow.AddDays(7);
            Response.Cookies.Append("refresh_token", tokens.RefreshToken, cookieOptions);

            Response.Cookies.Append("csrf_token", csrfToken, new CookieOptions
            {
                HttpOnly = false, // false is set for JS acess purposes
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(Convert.ToDouble(_configuration["Csrf:TokenExpiryInDays"])),
                Path = "/",
                Secure = true, // false is set for develop. Set to true for production
                IsEssential = true
            });

            _logger.LogInformation("Cookies set: access_token, refresh_token, csrf_token");
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

            // TODO: Add email confirmation logic here

            _logger.LogInformation("User {Email} registered successfully",
                model.Email);
            return Created();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            // TODO: Limit login attempts

            var csrfTokenFromHeader = Request.Headers["X-CSRF-Token"].FirstOrDefault();
            var csrfTokenFromCookie = Request.Cookies["csrf_token"];
            var existingUser = await _userManager.FindByEmailAsync(model.Email);

            // (optional) Check CSRF token if provided
            if (!string.IsNullOrEmpty(csrfTokenFromHeader) && 
                !string.IsNullOrEmpty(csrfTokenFromCookie) &&
                existingUser != null &&
                !await _csrfTokenService.ValidateCsrfTokenAsync(csrfTokenFromCookie, existingUser.Id))
            {
                _logger.LogWarning("Invalid CSRF token for user: {Email}",
                    model.Email);
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Errors = new[] { "Недействительный CSRF токен" }
                });
            }

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

            // Generate CSRF token
            var csrfToken = await _csrfTokenService.GenerateCsrfTokenAsync(existingUser.Id);

            // Add tokens to cookies
            setAuthCookies(tokens, csrfToken);

            return Ok(new AuthResponse() { Success = true });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var accessToken = Request.Cookies["access_token"];
            var refreshToken = Request.Cookies["refresh_token"];

            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("Access token is not provided.");
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Errors = new[] { "Токен доступа не предоставлен" }
                });
            }

            if (string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogWarning("Refresh token is not provided.");
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Errors = new[] { "Токен обновления не предоставлен" }
                });
            }

            // Get refresh token from DB
            var storedRefreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);
            if (storedRefreshToken == null)
            {
                _logger.LogWarning("Refresh token not found in database");
                return Unauthorized(new AuthResponse
                {
                    Success = false,
                    Errors = new[] { "Недействительный refresh token" }
                });
            }
            // Get userId using stored token
            var userId = storedRefreshToken.UserId;
            var tokenRequest = new TokenRequest { RefreshToken = refreshToken, AccessToken = accessToken };
            // Verify and generate new tokens
            var result = await _tokenService.VerifyAndGenerateTokenAsync(tokenRequest);
            if (!result.Success)
            {
                _logger.LogWarning("Token verification failed for user: {UserId}", userId);
                return Unauthorized(new AuthResponse
                {
                    Success = false,
                    Errors = new[] { "Ошибка валидации токена" }
                });
            }

            // Generate new csrf-token
            var newCsrfToken = await _csrfTokenService.GenerateCsrfTokenAsync(userId);

            // Set cookies
            setAuthCookies(result, newCsrfToken);

            _logger.LogInformation("Refresh token successfully rotated for user: {UserId}", userId);
            return Ok(new AuthResponse { Success = true });
        }

        [Authorize]
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request)
        {
            var csrfToken = Request.Cookies["csrf_token"];
            var userId = User.FindFirst("id")?.Value;

            if (string.IsNullOrEmpty(csrfToken) || !await _csrfTokenService.ValidateCsrfTokenAsync(csrfToken, userId))
            {
                _logger.LogWarning("CSRF validation failed for revoke token request, UserId: {UserId}", userId);
                return BadRequest(new RevokeTokenResponse
                {
                    Success = false,
                    Errors = new[] { "Недействительный CSRF-токен" }
                });
            }

            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                _logger.LogWarning("Refresh token not provided for revoke request, UserId: {UserId}", userId);
                return BadRequest(new RevokeTokenResponse
                {
                    Success = false,
                    Errors = new[] { "Некорректные данные токена" }
                });
            }

            var result = await _tokenService.RevokeTokenAsync(request.RefreshToken);
            if (!result)
            {
                _logger.LogWarning("Failed to revoke token for user: {UserId}", userId);
                return BadRequest(new RevokeTokenResponse
                {
                    Success = false,
                    Errors = new[] { "Не удалось отозвать токен" }
                });
            }

            _logger.LogInformation("Token revoked successfully for user: {UserId}", userId);
            return Ok(new RevokeTokenResponse { Success = true });
        }

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
