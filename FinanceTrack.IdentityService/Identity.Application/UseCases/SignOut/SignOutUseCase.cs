using System;
using System.Threading.Tasks;
using Idenitity.Application.Enums;
using Idenitity.Application.Ports.Repositories;
using Idenitity.Application.UseCases.SignOut.Request;
using Idenitity.Application.UseCases.SignOut.Response;
using Identity.Application.Enums;
using Identity.Application.Ports.Repositories;
using Microsoft.Extensions.Logging;

namespace Idenitity.Application.UseCases.SignOut
{
    public class SignOutUseCase : ISignOutUseCase
    {
        private readonly ILogger _logger;
        private readonly IIdentityRepository _authRepo;

        public SignOutUseCase(ILogger<SignOutUseCase> logger, IIdentityRepository authRepo)
        {
            _logger = logger;
            _authRepo = authRepo;
        }

        public async Task<SignOutResponse> Execute(SignOutRequest request)
        {
            try
            {
                var user = await _authRepo.GetUserByUserId(request.UserId);
                if (user == null)
                {
                    return new SignOutErrorResponse
                    {
                        Message = Enum.GetName(ErrorCodes.UserDoesNotExist),
                        Code = ErrorCodes.UserDoesNotExist.ToString("D"),
                    };
                }
                user.RefreshToken.Active = false;
                await _authRepo.UpdateUser(user);

                return new SignOutSuccessResponse
                {
                    Message = $"User signed out at {DateTime.UtcNow} server time.",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new SignOutErrorResponse
                {
                    Code = "Some Error Code",
                    Message = "Some Error Message",
                };
            }
        }
    }
}
