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

namespace DND5E_CE.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ITokenService _tokenService;

        public AuthController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ITokenService tokenService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            // Check model validity
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponse()
                {
                    Success = false,
                    Errors = new[] { "Некорректные регистрационные данные" }
                });
            }

            // Create new user
            var user = new IdentityUser { UserName=model.Username, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
                return Created();

            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            // Check model validity
            if (!ModelState.IsValid)
            {
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
                return BadRequest(new AuthResponse()
                {
                    Success = false,
                    Errors = new[] { "Неверная почта или пароль" }
                });
            }
                
            // Check password
            var isCorrect = await _userManager.CheckPasswordAsync(existingUser, model.Password);
            if (!isCorrect)
            {
                return BadRequest(new AuthResponse()
                {
                    Success = false,
                    Errors = new[] { "Неверная почта или пароль" }
                });
            }
            
            // Generate tokens
            var tokens = await _tokenService.GenerateTokensAsync(existingUser);
            return Ok(tokens);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequest tokenRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Errors = new[] { "Некорректные данные токена" }
                });
            }
            // Check and update tokens
            var result = await _tokenService.VerifyAndGenerateTokenAsync(tokenRequest);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [Authorize]
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest(new RevokeTokenResponse
                {
                    Success = false,
                    Errors = new[] { "Некорректные данные токена" }
                });
            }

            var result = await _tokenService.RevokeTokenAsync(request.RefreshToken);
            if (!result)
            {
                return BadRequest(new RevokeTokenResponse
                {
                    Success = false,
                    Errors =  new[] { "Не удалось отозвать токен" }
                });
            }

            return Ok(new RevokeTokenResponse { Success = true });
        }
    }
}
