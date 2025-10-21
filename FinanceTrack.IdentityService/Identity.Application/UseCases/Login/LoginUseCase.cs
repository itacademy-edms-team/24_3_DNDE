using System;
using System.Threading.Tasks;
using Identity.Application.Enums;
using Identity.Application.Exceptions;
using Identity.Application.Ports.Repositories;
using Identity.Application.Ports.Services;
using Identity.Application.UseCases.CreateUser.Response;
using Identity.Application.UseCases.Login;
using Identity.Application.UseCases.Login.Request;
using Identity.Application.UseCases.Login.Response;
using Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Idenitity.Application.UseCases.Login
{
    public class LoginUseCase : ILoginUseCase
    {
        private readonly ILogger _logger;
        private readonly IIdentityRepository _authRepo;
        private readonly ITokenRepository _tokenRepo;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IAuthTokenService _jwtService;

        public LoginUseCase(
            ILogger<LoginUseCase> logger,
            IIdentityRepository authRepo,
            ITokenRepository tokenRepo,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IAuthTokenService jwtService
        )
        {
            _logger = logger;
            _authRepo = authRepo;
            _tokenRepo = tokenRepo;
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
        }

        public async Task<LoginResponse> Execute(LoginRequest request)
        {
            try
            {
                var user = await _authRepo.GetUserByEmail(request.Email);

                if (user == null)
                {
                    throw new LoginUserException("User not found");
                }

                var signInResult = await _signInManager.CheckPasswordSignInAsync(
                    user,
                    request.Password,
                    false
                );
                if (!signInResult.Succeeded)
                {
                    return new LoginErrorResponse
                    {
                        Message = Enum.GetName(ErrorCodes.CredentialsAreNotValid),
                        Code = ErrorCodes.CredentialsAreNotValid.ToString("D"),
                    };
                }

                // Tokens
                var accessToken = await _jwtService.GenerateAccessToken(user);
                var refreshToken = await _jwtService.GenerateRefreshToken(user);
                _tokenRepo.CreateRefreshToken(user.Id, refreshToken);

                return new LoginSuccessResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                };
            }
            catch (LoginUserException ex)
            {
                _logger.LogWarning(ex, "Login failed for email: {Email}", request.Email);
                return new LoginErrorResponse
                {
                    Message = ex.Message,
                    Code = ErrorCodes.UserLoginFailed.ToString("D"),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error during login for email: {Email}",
                    request.Email
                );
                return new LoginErrorResponse
                {
                    Message = Enum.GetName(ErrorCodes.AnUnexpectedErrorOcurred),
                    Code = ErrorCodes.AnUnexpectedErrorOcurred.ToString("D"),
                };
            }
        }
    }
}
