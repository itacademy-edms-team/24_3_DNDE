using System.Data.Common;
using Identity.Application.Commands.SignUpUser.Request;
using Identity.Application.Commands.SignUpUser.Response;
using Identity.Application.Exceptions;
using Identity.Application.Ports.Repositories;
using Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Commands.SignUpUser
{
    public class SignUpUserCommand(ILogger<SignUpUserCommand> logger, UserManager<User> userManager)
        : ISignUpUserCommand
    {
        public async Task<SignUpUserResponse> Execute(SignUpUserRequest request)
        {
            // Password != PasswordConfirm
            if (request.Password != request.PasswordConfirm)
            {
                logger.LogWarning("Sign-up attempt with password != passwordConfirm");
                return new SignUpUserResponse
                {
                    IsSuccess = false,
                    Error = new ProblemDetails
                    {
                        Status = 400,
                        Title = "Invalid Input",
                        Detail = "Passwords do not match",
                    },
                };
            }

            try
            {
                // Check if user already exist
                var existingUser = await userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    logger.LogWarning("Sign-up attempt using existing email");
                    return new SignUpUserResponse
                    {
                        IsSuccess = false,
                        Error = new ProblemDetails
                        {
                            Status = 409,
                            Title = "Conflict",
                            Detail = "Email already exists",
                        },
                    };
                }

                // Create user
                var user = new User { UserName = request.Email, Email = request.Email };
                var result = await userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    return new SignUpUserResponse
                    {
                        IsSuccess = false,
                        Error = new ProblemDetails
                        {
                            Status = 400,
                            Title = "Invalid Input",
                            Detail = errors,
                        },
                    };
                }

                // Add user role
                await userManager.AddToRoleAsync(user, "User");

                return new SignUpUserResponse { IsSuccess = true, UserId = user.Id.ToString() };
            }
            catch (DbException ex)
            {
                logger.LogError(ex, "Database error during user creation: {Email}", request.Email);
                return new SignUpUserResponse
                {
                    IsSuccess = false,
                    Error = new ProblemDetails
                    {
                        Status = 500,
                        Title = "Server Error",
                        Detail = "Database error occurred",
                    },
                };
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Unexpected error during user creation: {Email}",
                    request.Email
                );
                return new SignUpUserResponse
                {
                    IsSuccess = false,
                    Error = new ProblemDetails
                    {
                        Status = 500,
                        Title = "Server Error",
                        Detail = "An unexpected error occurred",
                    },
                };
            }
        }
    }
}
