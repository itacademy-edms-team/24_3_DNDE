using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Identity.Application.Commands.SignInUser;
using Identity.Application.Commands.SignInUser.Request;
using Identity.Application.Exceptions;
using Identity.Application.Ports.Services;
using Identity.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Identity.Application.UnitTests
{
    public class SignInUserCommandTests
    {
        private static Mock<UserManager<User>> CreateUserManagerMock()
        {
            var store = new Mock<IUserStore<User>>();

            return new Mock<UserManager<User>>(
                store.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null
            );
        }

        [Fact]
        public async Task Execute_Returns401_WhenUserNotFound()
        {
            // Arrange
            var req = new SignInUserRequest { Email = "nosuch@example.com", Password = "whatever" };

            var userManagerMock = CreateUserManagerMock();
            userManagerMock
                .Setup(m => m.FindByEmailAsync("nosuch@example.com"))
                .ReturnsAsync((User?)null);

            var passwordSignInMock = new Mock<IUserPasswordSignInService>();
            var authTokenServiceMock = new Mock<IAuthTokenService>();

            var logger = NullLogger<SignInUserCommand>.Instance;

            var cmd = new SignInUserCommand(
                logger,
                userManagerMock.Object,
                passwordSignInMock.Object,
                authTokenServiceMock.Object
            );

            // Act
            var result = await cmd.Execute(req);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Status.Should().Be(401);
            result.Error.Title.Should().Be("Unauthorized");
            result.Error.Detail.Should().Be("Invalid email or password");

            // Check no token generation calls
            passwordSignInMock.Verify(
                s => s.CheckPassword(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<bool>()),
                Times.Never
            );
            authTokenServiceMock.Verify(s => s.GenerateAccessToken(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Execute_Returns401_WhenPasswordIsInvalid()
        {
            // Arrange
            var req = new SignInUserRequest { Email = "user@example.com", Password = "BadPass" };

            var userManagerMock = CreateUserManagerMock();
            var fakeUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "user@example.com",
                UserName = "user@example.com",
            };

            userManagerMock
                .Setup(m => m.FindByEmailAsync("user@example.com"))
                .ReturnsAsync(fakeUser);

            var passwordSignInMock = new Mock<IUserPasswordSignInService>();
            passwordSignInMock
                .Setup(s => s.CheckPassword(fakeUser, "BadPass", false))
                .ReturnsAsync(SignInResult.Failed);

            var authTokenServiceMock = new Mock<IAuthTokenService>();

            var logger = NullLogger<SignInUserCommand>.Instance;

            var cmd = new SignInUserCommand(
                logger,
                userManagerMock.Object,
                passwordSignInMock.Object,
                authTokenServiceMock.Object
            );

            // Act
            var result = await cmd.Execute(req);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Status.Should().Be(401);
            result.Error.Title.Should().Be("Unauthorized");
            result.Error.Detail.Should().Be("Invalid email or password");

            authTokenServiceMock.Verify(s => s.GenerateAccessToken(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Execute_ReturnsTokens_WhenPasswordIsValid()
        {
            // Arrange
            var req = new SignInUserRequest
            {
                Email = "user@example.com",
                Password = "CorrectPass123!",
            };

            var userManagerMock = CreateUserManagerMock();
            var fakeUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "user@example.com",
                UserName = "user@example.com",
            };

            userManagerMock
                .Setup(m => m.FindByEmailAsync("user@example.com"))
                .ReturnsAsync(fakeUser);

            var passwordSignInMock = new Mock<IUserPasswordSignInService>();
            passwordSignInMock
                .Setup(s => s.CheckPassword(fakeUser, "CorrectPass123!", false))
                .ReturnsAsync(SignInResult.Success);

            var authTokenServiceMock = new Mock<IAuthTokenService>();
            authTokenServiceMock
                .Setup(s => s.GenerateAccessToken(fakeUser))
                .ReturnsAsync("ACCESS_123");
            authTokenServiceMock
                .Setup(s => s.GenerateRefreshToken(fakeUser))
                .ReturnsAsync("REFRESH_456");

            var logger = NullLogger<SignInUserCommand>.Instance;

            var cmd = new SignInUserCommand(
                logger,
                userManagerMock.Object,
                passwordSignInMock.Object,
                authTokenServiceMock.Object
            );

            // Act
            var result = await cmd.Execute(req);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.AccessToken.Should().Be("ACCESS_123");
            result.RefreshToken.Should().Be("REFRESH_456");
            result.Error.Should().BeNull();

            authTokenServiceMock.Verify(s => s.GenerateAccessToken(fakeUser), Times.Once);
            authTokenServiceMock.Verify(s => s.GenerateRefreshToken(fakeUser), Times.Once);
        }

        [Fact]
        public async Task Execute_Returns500_WhenRefreshTokenGenerationFails()
        {
            // Arrange
            var req = new SignInUserRequest
            {
                Email = "user@example.com",
                Password = "CorrectPass123!",
            };

            var userManagerMock = CreateUserManagerMock();
            var fakeUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "user@example.com",
                UserName = "user@example.com",
            };

            userManagerMock
                .Setup(m => m.FindByEmailAsync("user@example.com"))
                .ReturnsAsync(fakeUser);

            var passwordSignInMock = new Mock<IUserPasswordSignInService>();
            passwordSignInMock
                .Setup(s => s.CheckPassword(fakeUser, "CorrectPass123!", false))
                .ReturnsAsync(SignInResult.Success);

            var authTokenServiceMock = new Mock<IAuthTokenService>();
            authTokenServiceMock
                .Setup(s => s.GenerateAccessToken(fakeUser))
                .ReturnsAsync("ACCESS_123");

            // модель: падает при создании refresh токена
            authTokenServiceMock
                .Setup(s => s.GenerateRefreshToken(fakeUser))
                .ThrowsAsync(new RefreshTokenRepositoryException("db fail"));

            var logger = NullLogger<SignInUserCommand>.Instance;

            var cmd = new SignInUserCommand(
                logger,
                userManagerMock.Object,
                passwordSignInMock.Object,
                authTokenServiceMock.Object
            );

            // Act
            var result = await cmd.Execute(req);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Status.Should().Be(500);
            result.Error.Title.Should().Be("Server Error");
            result.Error.Detail.Should().Be("Database error occurred");

            result.AccessToken.Should().BeNullOrEmpty();
            result.RefreshToken.Should().BeNullOrEmpty();
        }
    }
}
