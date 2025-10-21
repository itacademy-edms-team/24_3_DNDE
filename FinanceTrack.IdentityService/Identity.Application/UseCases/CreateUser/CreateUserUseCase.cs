using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Identity.Application.Enums;
using Identity.Application.Exceptions;
using Identity.Application.Ports.Repositories;
using Identity.Application.Ports.Services;
using Identity.Application.UseCases.CreateUser.Request;
using Identity.Application.UseCases.CreateUser.Response;
using Identity.Domain;
using Microsoft.Extensions.Logging;

namespace Identity.Application.UseCases.CreateUser
{
    public class CreateUserUseCase : ICreateUserUseCase
    {
        private readonly ILogger _logger;
        IIdentityRepository _repo;

        public CreateUserUseCase(ILogger<CreateUserUseCase> logger, IIdentityRepository repo)
        {
            _logger = logger;
            _repo = repo;
        }

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
                var user = await _repo.CreateUser(
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
                _logger.LogWarning(ex, "User creation failed: {Email}", request.Email);
                return new CreateUserErrorResponse
                {
                    Message = ex.Message,
                    Code = ErrorCodes.UserCreationFailed.ToString("D"),
                };
            }
            catch (DbException ex)
            {
                _logger.LogError(ex, "Database error during user creation: {Email}", request.Email);
                return new CreateUserErrorResponse
                {
                    Message = "Database error occurred",
                    Code = ErrorCodes.DatabaseError.ToString("D"),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
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
