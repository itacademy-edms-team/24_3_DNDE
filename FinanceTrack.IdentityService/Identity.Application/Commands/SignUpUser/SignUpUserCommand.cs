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
            // request == null or empty body
            if (request == null)
            {
                logger.LogWarning("Sign-up attempt with null request body");
                return new SignUpUserResponse
                {
                    IsSuccess = false,
                    Error = new ProblemDetails
                    {
                        Status = 400,
                        Title = "ValidationError",
                        Detail = "Request body is required.",
                    },
                };
            }

            // Password != PasswordConfirm
            if (request.Password != request.PasswordConfirm)
            {
                logger.LogWarning(
                    "Sign-up attempt with password mismatch for {Email}",
                    request.Email
                );

                return new SignUpUserResponse
                {
                    IsSuccess = false,
                    Error = new ProblemDetails
                    {
                        Status = 400,
                        Title = "ValidationError",
                        Detail = "Passwords do not match.",
                    },
                };
            }

            try
            {
                // Check if user already exists
                var existingUser = await userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    logger.LogWarning(
                        "Sign-up attempt using existing email {Email}",
                        request.Email
                    );

                    return new SignUpUserResponse
                    {
                        IsSuccess = false,
                        Error = new ProblemDetails
                        {
                            Status = 409,
                            Title = "Conflict",
                            Detail = "Email already exists.",
                        },
                    };
                }

                // Create user
                var user = new User { UserName = request.Email, Email = request.Email };

                var createResult = await userManager.CreateAsync(user, request.Password);

                if (!createResult.Succeeded)
                {
                    var errors = string.Join(
                        "; ",
                        createResult.Errors.Select(e => $"{e.Code}: {e.Description}")
                    );

                    logger.LogWarning(
                        "User creation failed for {Email}: {Errors}",
                        request.Email,
                        errors
                    );

                    return new SignUpUserResponse
                    {
                        IsSuccess = false,
                        Error = new ProblemDetails
                        {
                            Status = 400,
                            Title = "ValidationError",
                            Detail = errors,
                        },
                    };
                }

                // Assign default role
                var addToRoleResult = await userManager.AddToRoleAsync(user, "User");
                if (!addToRoleResult.Succeeded)
                {
                    var roleErrors = string.Join(
                        "; ",
                        addToRoleResult.Errors.Select(e => $"{e.Code}: {e.Description}")
                    );

                    logger.LogError(
                        "Failed to assign default role 'User' for {Email}: {RoleErrors}",
                        request.Email,
                        roleErrors
                    );

                    await userManager.DeleteAsync(user); // Delete broken user

                    return new SignUpUserResponse
                    {
                        IsSuccess = false,
                        Error = new ProblemDetails
                        {
                            Status = 500,
                            Title = "ServerError",
                            Detail = "Failed to finalize user creation.",
                        },
                    };
                }

                logger.LogInformation(
                    "User {UserId} successfully registered with email {Email}",
                    user.Id,
                    request.Email
                );

                return new SignUpUserResponse { IsSuccess = true, UserId = user.Id.ToString() };
            }
            catch (DbException ex)
            {
                logger.LogError(
                    ex,
                    "Database error during user creation for {Email}",
                    request.Email
                );

                return new SignUpUserResponse
                {
                    IsSuccess = false,
                    Error = new ProblemDetails
                    {
                        Status = 500,
                        Title = "DatabaseError",
                        Detail = "A database error occurred.",
                    },
                };
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Unexpected error during user creation for {Email}",
                    request.Email
                );

                return new SignUpUserResponse
                {
                    IsSuccess = false,
                    Error = new ProblemDetails
                    {
                        Status = 500,
                        Title = "UnexpectedError",
                        Detail = "An unexpected error occurred.",
                    },
                };
            }
        }
    }
}
