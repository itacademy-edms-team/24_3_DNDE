using System;
using System.Threading.Tasks;
using Idenitity.Application.Commands.SignOut;
using Idenitity.Application.Commands.SignOut.Request;
using Idenitity.Application.Commands.SignOut.Response;
using Identity.Application.Enums;
using Identity.Application.Ports.Repositories;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Commands.SignOut
{
    public class SignOutCommand(ILogger<SignOutCommand> logger, IIdentityRepository authRepo)
        : ISignOutCommand
    {
        public async Task<SignOutResponse> Execute(SignOutRequest request)
        {
            try
            {
                var user = await authRepo.GetUserByUserId(request.UserId);
                if (user == null)
                {
                    return new SignOutErrorResponse
                    {
                        Message = Enum.GetName(ErrorCodes.UserDoesNotExist),
                        Code = ErrorCodes.UserDoesNotExist.ToString("D"),
                    };
                }
                user.RefreshToken.Active = false;
                await authRepo.UpdateUser(user);

                return new SignOutSuccessResponse
                {
                    Message = $"User signed out at {DateTime.UtcNow} server time.",
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return new SignOutErrorResponse
                {
                    Message = Enum.GetName(ErrorCodes.AnUnexpectedErrorOcurred),
                    Code = ErrorCodes.AnUnexpectedErrorOcurred.ToString("D"),
                };
            }
        }
    }
}
