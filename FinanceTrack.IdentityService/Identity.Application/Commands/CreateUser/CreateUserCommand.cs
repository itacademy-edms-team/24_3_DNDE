using System.Data.Common;
using Identity.Application.Commands.CreateUser.Request;
using Identity.Application.Commands.CreateUser.Response;
using Identity.Application.Enums;
using Identity.Application.Exceptions;
using Identity.Application.Ports.Repositories;
using Identity.Domain;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Commands.CreateUser
{
    public class CreateUserCommand(ILogger<CreateUserCommand> logger, IIdentityRepository repo)
        : ICreateUserCommand
    {
        public async Task<CreateUserResponse> Execute(CreateUserRequest request)
        {
            if (request.Password != request.PasswordConfirm)
            {
                return new CreateUserErrorResponse
                {
                    Message = Enum.GetName(ErrorCodes.PasswordsDoNotMatch),
                    Code = ErrorCodes.PasswordsDoNotMatch.ToString("D"),
                };
            }

            try
            {
                var user = await repo.CreateUser(
                    new User { UserName = request.Email, Email = request.Email },
                    request.Password
                );

                if (user == null)
                {
                    throw new CreateUserException("User creation failed for an unknown reason.");
                }

                return new CreateUserSuccessResponse { UserId = user.Id };
            }
            catch (CreateUserException ex)
            {
                logger.LogWarning(ex, "User creation failed: {Email}", request.Email);
                return new CreateUserErrorResponse
                {
                    Message = ex.Message,
                    Code = ErrorCodes.UserCreationFailed.ToString("D"),
                };
            }
            catch (DbException ex)
            {
                logger.LogError(ex, "Database error during user creation: {Email}", request.Email);
                return new CreateUserErrorResponse
                {
                    Message = "Database error occurred",
                    Code = ErrorCodes.DatabaseError.ToString("D"),
                };
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Unexpected error during user creation: {Email}",
                    request.Email
                );
                return new CreateUserErrorResponse
                {
                    Message = Enum.GetName(ErrorCodes.AnUnexpectedErrorOcurred),
                    Code = ErrorCodes.AnUnexpectedErrorOcurred.ToString("D"),
                };
            }
        }
    }
}
