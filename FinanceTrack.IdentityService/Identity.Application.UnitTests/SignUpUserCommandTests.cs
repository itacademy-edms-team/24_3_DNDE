using System.Threading.Tasks;
using FluentAssertions;
using Identity.Application.Commands.SignUpUser;
using Identity.Application.Commands.SignUpUser.Request;
using Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Identity.Application.UnitTests
{
    public class SignUpUserCommandTests
    {
        private static Mock<UserManager<User>> CreateUserManagerMock()
        {
            var store = new Mock<IUserStore<User>>();

            return new Mock<UserManager<User>>(
                store.Object,
                null, // IOptions<IdentityOptions>
                null, // IPasswordHasher<User>
                null, // IEnumerable<IUserValidator<User>>
                null, // IEnumerable<IPasswordValidator<User>>
                null, // ILookupNormalizer
                null, // IdentityErrorDescriber
                null, // IServiceProvider
                null // ILogger<UserManager<User>>
            );
        }

        [Fact]
        public async Task Execute_Returns400_WhenRequestIsNull()
        {
            // Arrange
            var userManagerMock = CreateUserManagerMock();
            var logger = NullLogger<SignUpUserCommand>.Instance;

            var command = new SignUpUserCommand(logger, userManagerMock.Object);

            // Act
            var result = await command.Execute(null);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Status.Should().Be(400);
            result.Error.Title.Should().Be("ValidationError");
            result.Error.Detail.Should().Be("Request body is required.");
        }

        [Fact]
        public async Task Execute_Returns400_WhenPasswordsDontMatch()
        {
            // Arrange
            var request = new SignUpUserRequest
            {
                Email = "example@mail.com",
                Password = "Super_Stron9_password",
                PasswordConfirm = "Not_Equal_password",
            };

            var userManagerMock = CreateUserManagerMock();
            var logger = NullLogger<SignUpUserCommand>.Instance;

            var command = new SignUpUserCommand(logger, userManagerMock.Object);

            // Act
            var result = await command.Execute(request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Status.Should().Be(400);
            result.Error.Title.Should().Be("ValidationError");
            result.Error.Detail.Should().Be("Passwords do not match.");

            //// Check if userManager.CreateAsync never called
            userManagerMock.Verify(
                m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()),
                Times.Never
            );
        }

        [Fact]
        public async Task Execute_Retursn409_WhenEmailAlreadyExists()
        {
            // Arrange
            var request = new SignUpUserRequest
            {
                Email = "existing@example.com",
                Password = "Super_Stron9_password",
                PasswordConfirm = "Super_Stron9_password",
            };

            var userManagerMock = CreateUserManagerMock();
            userManagerMock
                .Setup(m => m.FindByEmailAsync("existing@example.com"))
                .ReturnsAsync(
                    new User
                    {
                        Id = Guid.NewGuid(),
                        Email = "existing@example.com",
                        UserName = "existing@example.com",
                    }
                );

            var logger = NullLogger<SignUpUserCommand>.Instance;

            var command = new SignUpUserCommand(logger, userManagerMock.Object);

            // Act
            var result = await command.Execute(request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Status.Should().Be(409);
            result.Error.Title.Should().Be("Conflict");
            result.Error.Detail.Should().Be("Email already exists.");

            //// Check if userManager.CreateAsync never called
            userManagerMock.Verify(
                m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()),
                Times.Never
            );
        }

        [Fact]
        public async Task Execute_Retursn400_WhenUserManagerCreateFails()
        {
            // Arrange
            var request = new SignUpUserRequest
            {
                Email = "newuser@example.com",
                Password = "weakPass",
                PasswordConfirm = "weakPass",
            };

            var userManagerMock = CreateUserManagerMock();

            // user is not exist
            userManagerMock
                .Setup(m => m.FindByEmailAsync("newuser@example.com"))
                .ReturnsAsync((User?)null);

            var identityErrors = new[]
            {
                new IdentityError
                {
                    Code = "PasswordTooShort",
                    Description = "Password too short.",
                },
                new IdentityError
                {
                    Code = "PasswordRequiresDigit",
                    Description = "Needs a digit.",
                },
            };

            userManagerMock
                .Setup(m => m.CreateAsync(It.IsAny<User>(), "weakPass"))
                .ReturnsAsync(IdentityResult.Failed(identityErrors));

            var logger = NullLogger<SignUpUserCommand>.Instance;

            var command = new SignUpUserCommand(logger, userManagerMock.Object);

            // Act
            var result = await command.Execute(request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Status.Should().Be(400);
            result.Error.Title.Should().Be("ValidationError");

            result.Error.Detail.Should().Contain("PasswordTooShort");
            result.Error.Detail.Should().Contain("Password too short.");
            result.Error.Detail.Should().Contain("PasswordRequiresDigit");
            result.Error.Detail.Should().Contain("Needs a digit.");

            //// AddToRoleAsync should not been called
            userManagerMock.Verify(
                m => m.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()),
                Times.Never
            );
        }

        [Fact]
        public async Task Execute_Returns500_WhenAddToRoleFails()
        {
            // Arrange
            var req = new SignUpUserRequest
            {
                Email = "newuser@example.com",
                Password = "Qwerty123!",
                PasswordConfirm = "Qwerty123!",
            };

            var userManagerMock = CreateUserManagerMock();

            // user never exist
            userManagerMock
                .Setup(m => m.FindByEmailAsync("newuser@example.com"))
                .ReturnsAsync((User?)null);
            var uGuid = Guid.NewGuid();
            userManagerMock
                .Setup(m => m.CreateAsync(It.IsAny<User>(), "Qwerty123!"))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<User, string>(
                    (u, _) =>
                    {
                        u.Id = uGuid;
                    }
                );

            var roleErrors = new[]
            {
                new IdentityError { Code = "RoleMissing", Description = "Role User not found." },
            };

            userManagerMock
                .Setup(m => m.AddToRoleAsync(It.IsAny<User>(), "User"))
                .ReturnsAsync(IdentityResult.Failed(roleErrors));

            var logger = NullLogger<SignUpUserCommand>.Instance;
            var command = new SignUpUserCommand(logger, userManagerMock.Object);

            // Act
            var result = await command.Execute(req);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Status.Should().Be(500);
            result.Error.Title.Should().Be("ServerError");
            result.Error.Detail.Should().Be("Failed to finalize user creation.");

            //// Verify DeleteAsync called
            userManagerMock.Verify(m => m.DeleteAsync(It.Is<User>(u => u.Id == uGuid)), Times.Once);
        }

        [Fact]
        public async Task Execute_ReturnsSuccess_WhenAllStepsPass()
        {
            // Arrange
            var req = new SignUpUserRequest
            {
                Email = "newuser@example.com",
                Password = "Qwerty123!",
                PasswordConfirm = "Qwerty123!",
            };

            var userManagerMock = CreateUserManagerMock();

            // user never exist
            userManagerMock
                .Setup(m => m.FindByEmailAsync("newuser@example.com"))
                .ReturnsAsync((User?)null);
            var uGuid = Guid.NewGuid();
            userManagerMock
                .Setup(m => m.CreateAsync(It.IsAny<User>(), "Qwerty123!"))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<User, string>(
                    (u, _) =>
                    {
                        u.Id = uGuid;
                    }
                );

            // Add role success
            userManagerMock
                .Setup(m => m.AddToRoleAsync(It.IsAny<User>(), "User"))
                .ReturnsAsync(IdentityResult.Success);

            var logger = NullLogger<SignUpUserCommand>.Instance;
            var command = new SignUpUserCommand(logger, userManagerMock.Object);

            // Act
            var result = await command.Execute(req);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.UserId.Should().Be(uGuid.ToString());
            result.Error.Should().BeNull();

            // Verify methods call order
            userManagerMock.Verify(m => m.FindByEmailAsync("newuser@example.com"), Times.Once);
            userManagerMock.Verify(m => m.CreateAsync(It.IsAny<User>(), "Qwerty123!"), Times.Once);
            userManagerMock.Verify(m => m.AddToRoleAsync(It.IsAny<User>(), "User"), Times.Once);
        }
    }
}
