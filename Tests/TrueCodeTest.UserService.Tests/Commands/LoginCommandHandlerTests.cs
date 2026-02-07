using Moq;
using Microsoft.Extensions.Configuration;
using TrueCodeTest.Shared.Domain.Entities;
using TrueCodeTest.UserService.Application.Commands;
using TrueCodeTest.UserService.Application.Commands.Login;
using TrueCodeTest.UserService.Domain.Interfaces;
using Xunit;

namespace TrueCodeTest.UserService.Tests.Commands;

public class LoginCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenCredentialsAreValid_ReturnsSuccessWithToken()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<LoginCommandHandler>>();
        var mockConfig = new Mock<IConfiguration>();
        
        mockConfig.Setup(c => c["Jwt:Key"]).Returns("a-string-secret-at-least-256-bits-long");
        mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("TrueCodeTest");
        mockConfig.Setup(c => c["Jwt:Audience"]).Returns("TrueCodeTest");

        // Hash password "TestPassword" using SHA256
        var passwordBytes = "TestPassword"u8.ToArray();
        var hashBytes = System.Security.Cryptography.SHA256.Create().ComputeHash(passwordBytes);
        var hashedPassword = Convert.ToBase64String(hashBytes);

        var user = new User { Id = 1, Name = "TestUser", Password = hashedPassword };
        mockRepository.Setup(r => r.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new LoginCommandHandler(mockRepository.Object, mockConfig.Object, mockLogger.Object);
        var command = new LoginCommand { Name = "TestUser", Password = "TestPassword" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Token);
        Assert.Equal(1, result.UserId);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ReturnsFailure()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<LoginCommandHandler>>();
        var mockConfig = new Mock<IConfiguration>();

        mockRepository.Setup(r => r.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var handler = new LoginCommandHandler(mockRepository.Object, mockConfig.Object, mockLogger.Object);
        var command = new LoginCommand { Name = "NonExistentUser", Password = "ValidPassword123" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Token);
        Assert.NotNull(result.ErrorMessage);
        Assert.Equal("Неверный логин или пароль", result.ErrorMessage);
    }

    [Fact]
    public async Task Handle_WhenPasswordIsIncorrect_ReturnsFailure()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<LoginCommandHandler>>();
        var mockConfig = new Mock<IConfiguration>();

        var user = new User { Id = 1, Name = "TestUser", Password = "WrongHashedPassword" };
        mockRepository.Setup(r => r.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new LoginCommandHandler(mockRepository.Object, mockConfig.Object, mockLogger.Object);
        var command = new LoginCommand { Name = "TestUser", Password = "TestPassword" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Token);
        Assert.NotNull(result.ErrorMessage);
        Assert.Equal("Неверный логин или пароль", result.ErrorMessage);
    }

    [Theory]
    [InlineData("")]
    [InlineData("a")] // меньше 6 символов
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")] // 101 символ
    public async Task Handle_WhenPasswordIsInvalid_ReturnsValidationFailure(string password)
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<LoginCommandHandler>>();
        var mockConfig = new Mock<IConfiguration>();

        var handler = new LoginCommandHandler(mockRepository.Object, mockConfig.Object, mockLogger.Object);
        var command = new LoginCommand { Name = "TestUser", Password = password };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Token);
        Assert.NotNull(result.ErrorMessage);
    }

    [Theory]
    [InlineData("")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    public async Task Handle_WhenUserNameIsInvalid_ReturnsValidationFailure(string userName)
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<LoginCommandHandler>>();
        var mockConfig = new Mock<IConfiguration>();

        var handler = new LoginCommandHandler(mockRepository.Object, mockConfig.Object, mockLogger.Object);
        var command = new LoginCommand { Name = userName, Password = "ValidPassword123" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Token);
        Assert.NotNull(result.ErrorMessage);
    }
}
