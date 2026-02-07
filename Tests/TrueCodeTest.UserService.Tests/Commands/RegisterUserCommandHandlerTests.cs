using Moq;
using TrueCodeTest.Shared.Domain.Entities;
using TrueCodeTest.UserService.Application.Commands;
using TrueCodeTest.UserService.Application.Commands.RegisterUser;
using TrueCodeTest.UserService.Domain.Interfaces;
using Xunit;

namespace TrueCodeTest.UserService.Tests.Commands;

public class RegisterUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ReturnsSuccess()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<RegisterUserCommandHandler>>();
        
        mockRepository.Setup(r => r.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        
        mockRepository.Setup(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User user, CancellationToken ct) => 
            {
                user.Id = 1;
                return user;
            });

        var handler = new RegisterUserCommandHandler(mockRepository.Object, mockLogger.Object);
        var command = new RegisterUserCommand { Name = "TestUser", Password = "TestPassword" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(1, result.UserId);
        Assert.Null(result.ErrorMessage);
        mockRepository.Verify(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserExists_ReturnsFailure()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<RegisterUserCommandHandler>>();
        
        var existingUser = new User { Id = 1, Name = "TestUser", Password = "HashedPassword" };
        mockRepository.Setup(r => r.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var handler = new RegisterUserCommandHandler(mockRepository.Object, mockLogger.Object);
        var command = new RegisterUserCommand { Name = "TestUser", Password = "TestPassword" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("Пользователь с таким именем уже существует", result.ErrorMessage);
    }

    [Theory]
    [InlineData("")]
    [InlineData("a")] // меньше 6 символов
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")] // 101 символ
    public async Task Handle_WhenPasswordIsInvalid_ReturnsValidationFailure(string password)
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<RegisterUserCommandHandler>>();

        var handler = new RegisterUserCommandHandler(mockRepository.Object, mockLogger.Object);
        var command = new RegisterUserCommand { Name = "TestUser", Password = password };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
    }

    [Theory]
    [InlineData("")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")] // 51 символ
    [InlineData("user@name")]
    public async Task Handle_WhenUserNameIsInvalid_ReturnsValidationFailure(string userName)
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<RegisterUserCommandHandler>>();

        var handler = new RegisterUserCommandHandler(mockRepository.Object, mockLogger.Object);
        var command = new RegisterUserCommand { Name = userName, Password = "ValidPassword123" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
    }
}
