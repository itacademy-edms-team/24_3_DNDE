using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Identity.Application.Commands.RefreshTokens;
using Identity.Application.Commands.RefreshTokens.Request;
using Identity.Application.Exceptions;
using Identity.Application.Ports.Repositories;
using Identity.Application.Ports.Services;
using Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Identity.Application.UnitTests
{
    public class RefreshTokensCommandTests
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
        public async Task Execute_Returns401_WhenRequestHasNoToken()
        {
            // Arrange
            var refreshRepoMock = new Mock<IRefreshTokenRepository>();
            var userManagerMock = CreateUserManagerMock();
            var authTokenServiceMock = new Mock<IAuthTokenService>();

            var command = new RefreshTokensCommand(
                NullLogger<RefreshTokensCommand>.Instance,
                refreshRepoMock.Object,
                userManagerMock.Object,
                authTokenServiceMock.Object
            );

            var req = new RefreshTokensRequest { RefreshToken = "" };

            // Act
            var result = await command.Execute(req);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Status.Should().Be(401);
            result.Error.Title.Should().Be("Unauthorized");
            result.Error.Detail.Should().Be("Refresh token is not provided");

            refreshRepoMock.Verify(r => r.GetRefreshTokenAsync(It.IsAny<string>()), Times.Never);
            authTokenServiceMock.Verify(
                a => a.GenerateAccessTokenAsync(It.IsAny<User>()),
                Times.Never
            );
        }

        [Fact]
        public async Task Execute_Returns401_WhenTokenNotFoundInRepository()
        {
            // Arrange
            var refreshRepoMock = new Mock<IRefreshTokenRepository>();
            var userManagerMock = CreateUserManagerMock();
            var authTokenServiceMock = new Mock<IAuthTokenService>();

            refreshRepoMock
                .Setup(r => r.GetRefreshTokenAsync("abc123"))
                .ReturnsAsync((RefreshToken?)null);

            var command = new RefreshTokensCommand(
                NullLogger<RefreshTokensCommand>.Instance,
                refreshRepoMock.Object,
                userManagerMock.Object,
                authTokenServiceMock.Object
            );

            var req = new RefreshTokensRequest { RefreshToken = "abc123" };

            // Act
            var result = await command.Execute(req);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Status.Should().Be(401);
            result.Error.Title.Should().Be("Unauthorized");
            result.Error.Detail.Should().Be("Refresh token is invalid");

            // Check if tokens generation commands never called
            userManagerMock.Verify(u => u.FindByIdAsync(It.IsAny<string>()), Times.Never);
            authTokenServiceMock.Verify(
                a => a.GenerateAccessTokenAsync(It.IsAny<User>()),
                Times.Never
            );
        }

        [Fact]
        public async Task Execute_Returns401_WhenTokenIsExpired()
        {
            // Arrange
            var expiredToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Token = "expired-token",
                Created = DateTime.UtcNow.AddMinutes(-30),
                Expires = DateTime.UtcNow.AddMinutes(-1), // уже истёк
                IsRevoked = false,
            };

            var refreshRepoMock = new Mock<IRefreshTokenRepository>();
            refreshRepoMock
                .Setup(r => r.GetRefreshTokenAsync("expired-token"))
                .ReturnsAsync(expiredToken);

            var userManagerMock = CreateUserManagerMock();
            var authTokenServiceMock = new Mock<IAuthTokenService>();

            var command = new RefreshTokensCommand(
                NullLogger<RefreshTokensCommand>.Instance,
                refreshRepoMock.Object,
                userManagerMock.Object,
                authTokenServiceMock.Object
            );

            var req = new RefreshTokensRequest { RefreshToken = "expired-token" };

            // Act
            var result = await command.Execute(req);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Status.Should().Be(401);
            result.Error.Detail.Should().Be("Refresh token is invalid");

            // Check if tokens generation commands never called
            authTokenServiceMock.Verify(
                a => a.GenerateAccessTokenAsync(It.IsAny<User>()),
                Times.Never
            );
            authTokenServiceMock.Verify(
                a => a.GenerateRefreshTokenAsync(It.IsAny<User>()),
                Times.Never
            );

            // Check if token revoke never called
            refreshRepoMock.Verify(r => r.RevokeTokenAsync(It.IsAny<RefreshToken>()), Times.Never);
        }

        [Fact]
        public async Task Execute_Returns401_WhenTokenIsRevoked()
        {
            // Arrange
            var revokedToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Token = "revoked-token",
                Created = DateTime.UtcNow.AddMinutes(-5),
                Expires = DateTime.UtcNow.AddMinutes(30), // alive
                IsRevoked = true, // but revoked
            };

            var refreshRepoMock = new Mock<IRefreshTokenRepository>();
            refreshRepoMock
                .Setup(r => r.GetRefreshTokenAsync("revoked-token"))
                .ReturnsAsync(revokedToken);

            var userManagerMock = CreateUserManagerMock();
            var authTokenServiceMock = new Mock<IAuthTokenService>();

            var command = new RefreshTokensCommand(
                NullLogger<RefreshTokensCommand>.Instance,
                refreshRepoMock.Object,
                userManagerMock.Object,
                authTokenServiceMock.Object
            );

            var req = new RefreshTokensRequest { RefreshToken = "revoked-token" };

            // Act
            var result = await command.Execute(req);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Status.Should().Be(401);
            result.Error.Detail.Should().Be("Refresh token is invalid");

            // User search should not be called
            userManagerMock.Verify(u => u.FindByIdAsync(It.IsAny<string>()), Times.Never);
            // Check if tokens generation commands never called
            authTokenServiceMock.Verify(
                a => a.GenerateAccessTokenAsync(It.IsAny<User>()),
                Times.Never
            );
            // Check if token revoke never called
            refreshRepoMock.Verify(r => r.RevokeTokenAsync(It.IsAny<RefreshToken>()), Times.Never);
        }

        [Fact]
        public async Task Execute_Returns401_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var validToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = "valid-token",
                Created = DateTime.UtcNow.AddMinutes(-5),
                Expires = DateTime.UtcNow.AddMinutes(30),
                IsRevoked = false,
            };

            var refreshRepoMock = new Mock<IRefreshTokenRepository>();
            refreshRepoMock
                .Setup(r => r.GetRefreshTokenAsync("valid-token"))
                .ReturnsAsync(validToken);

            var userManagerMock = CreateUserManagerMock();
            // User not found
            userManagerMock
                .Setup(u => u.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((User?)null);

            var authTokenServiceMock = new Mock<IAuthTokenService>();

            var command = new RefreshTokensCommand(
                NullLogger<RefreshTokensCommand>.Instance,
                refreshRepoMock.Object,
                userManagerMock.Object,
                authTokenServiceMock.Object
            );

            var req = new RefreshTokensRequest { RefreshToken = "valid-token" };

            // Act
            var result = await command.Execute(req);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Status.Should().Be(401);
            result.Error.Detail.Should().Be("User not found");

            // Check if tokens generation commands never called
            authTokenServiceMock.Verify(
                a => a.GenerateAccessTokenAsync(It.IsAny<User>()),
                Times.Never
            );
            // Check if token revoke never called
            refreshRepoMock.Verify(r => r.RevokeTokenAsync(It.IsAny<RefreshToken>()), Times.Never);
        }

        [Fact]
        public async Task Execute_Returns200_AndRotatesToken_WhenEverythingIsValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Email = "demo@example.com",
                UserName = "demo@example.com",
            };

            var validToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = "valid-live-token",
                Created = DateTime.UtcNow.AddMinutes(-1),
                Expires = DateTime.UtcNow.AddMinutes(60),
                IsRevoked = false,
            };

            var refreshRepoMock = new Mock<IRefreshTokenRepository>();
            refreshRepoMock
                .Setup(r => r.GetRefreshTokenAsync("valid-live-token"))
                .ReturnsAsync(validToken);

            var userManagerMock = CreateUserManagerMock();
            userManagerMock.Setup(u => u.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

            var authTokenServiceMock = new Mock<IAuthTokenService>();
            authTokenServiceMock
                .Setup(a => a.GenerateAccessTokenAsync(user))
                .ReturnsAsync("ACCESS_NEW_123");
            authTokenServiceMock
                .Setup(a => a.GenerateRefreshTokenAsync(user))
                .ReturnsAsync("REFRESH_NEW_456");

            var command = new RefreshTokensCommand(
                NullLogger<RefreshTokensCommand>.Instance,
                refreshRepoMock.Object,
                userManagerMock.Object,
                authTokenServiceMock.Object
            );

            var req = new RefreshTokensRequest { RefreshToken = "valid-live-token" };

            // Act
            var result = await command.Execute(req);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Error.Should().BeNull();
            result.AccessToken.Should().Be("ACCESS_NEW_123");
            result.RefreshToken.Should().Be("REFRESH_NEW_456");

            // Check if tokens generation commands called
            authTokenServiceMock.Verify(a => a.GenerateAccessTokenAsync(user), Times.Once);
            authTokenServiceMock.Verify(a => a.GenerateRefreshTokenAsync(user), Times.Once);

            // Check if token revoke called
            refreshRepoMock.Verify(r => r.RevokeTokenAsync(validToken), Times.Once);
        }

        [Fact]
        public async Task Execute_Returns500_WhenRepositoryThrows()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Email = "demo@example.com",
                UserName = "demo@example.com",
            };

            var validToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = "cause-error-token",
                Created = DateTime.UtcNow.AddMinutes(-1),
                Expires = DateTime.UtcNow.AddMinutes(60),
                IsRevoked = false,
            };

            var refreshRepoMock = new Mock<IRefreshTokenRepository>();
            refreshRepoMock
                .Setup(r => r.GetRefreshTokenAsync("cause-error-token"))
                .ReturnsAsync(validToken);

            // repo throws exception in RevokeTokenAsync -> DB problem simulation
            refreshRepoMock
                .Setup(r => r.RevokeTokenAsync(validToken))
                .ThrowsAsync(new RefreshTokenRepositoryException("db issue"));

            var userManagerMock = CreateUserManagerMock();
            userManagerMock.Setup(u => u.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

            var authTokenServiceMock = new Mock<IAuthTokenService>();
            authTokenServiceMock
                .Setup(a => a.GenerateAccessTokenAsync(user))
                .ReturnsAsync("ACCESS_NEW_AFTER_ERROR");
            authTokenServiceMock
                .Setup(a => a.GenerateRefreshTokenAsync(user))
                .ReturnsAsync("REFRESH_NEW_AFTER_ERROR");

            var command = new RefreshTokensCommand(
                NullLogger<RefreshTokensCommand>.Instance,
                refreshRepoMock.Object,
                userManagerMock.Object,
                authTokenServiceMock.Object
            );

            var req = new RefreshTokensRequest { RefreshToken = "cause-error-token" };

            // Act
            var result = await command.Execute(req);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Status.Should().Be(500);
            result.Error.Title.Should().Be("Server Error");
            result.Error.Detail.Should().Be("Database error occurred");
        }
    }
}
