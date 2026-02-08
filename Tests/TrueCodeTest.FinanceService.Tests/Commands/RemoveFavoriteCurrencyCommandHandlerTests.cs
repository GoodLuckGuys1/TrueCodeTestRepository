using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Moq;
using TrueCodeTest.FinanceService.Application.Commands.RemoveFavoriteCurrency;
using TrueCodeTest.FinanceService.Domain.Interfaces;
using TrueCodeTest.Shared.Domain.Data;
using TrueCodeTest.Shared.Domain.Entities;
using Xunit;

namespace TrueCodeTest.FinanceService.Tests.Commands;

public class RemoveFavoriteCurrencyCommandHandlerTests
{
    private ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Handle_WhenCurrencyExistsAndInFavorites_RemovesSuccessfully()
    {
        // Arrange
        var currency = new Currency { Id = 1, Name = "USD", Rate = 75.5m };
        var userCurrency = new UserCurrency { UserId = 1, CurrencyId = 1 };

        var mockCurrencyRepository = new Mock<ICurrencyRepository>();
        mockCurrencyRepository.Setup(r => r.GetByNameAsync("USD", It.IsAny<CancellationToken>()))
            .ReturnsAsync(currency);

        var mockUserCurrencyRepository = new Mock<IUserCurrencyRepository>();
        mockUserCurrencyRepository.Setup(r => r.GetByUserAndCurrencyAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userCurrency);

        var handler = new RemoveFavoriteCurrencyCommandHandler(
            mockCurrencyRepository.Object,
            mockUserCurrencyRepository.Object);
        var command = new RemoveFavoriteCurrencyCommand { UserId = 1, CurrencyName = "USD" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorMessage);

        mockUserCurrencyRepository.Verify(r => r.RemoveAsync(
            It.Is<UserCurrency>(uc => uc.UserId == 1 && uc.CurrencyId == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCurrencyNotFound_ReturnsError()
    {
        // Arrange
        var mockCurrencyRepository = new Mock<ICurrencyRepository>();
        mockCurrencyRepository.Setup(r => r.GetByNameAsync("UNKNOWN", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Currency?)null);

        var mockUserCurrencyRepository = new Mock<IUserCurrencyRepository>();

        var handler = new RemoveFavoriteCurrencyCommandHandler(
            mockCurrencyRepository.Object,
            mockUserCurrencyRepository.Object);
        var command = new RemoveFavoriteCurrencyCommand { UserId = 1, CurrencyName = "UNKNOWN" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Валюта не найдена", result.ErrorMessage);

        mockUserCurrencyRepository.Verify(r => r.RemoveAsync(It.IsAny<UserCurrency>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCurrencyNotInFavorites_ReturnsError()
    {
        // Arrange
        var currency = new Currency { Id = 1, Name = "USD", Rate = 75.5m };

        var mockCurrencyRepository = new Mock<ICurrencyRepository>();
        mockCurrencyRepository.Setup(r => r.GetByNameAsync("USD", It.IsAny<CancellationToken>()))
            .ReturnsAsync(currency);

        var mockUserCurrencyRepository = new Mock<IUserCurrencyRepository>();
        mockUserCurrencyRepository.Setup(r => r.GetByUserAndCurrencyAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserCurrency?)null);

        var handler = new RemoveFavoriteCurrencyCommandHandler(
            mockCurrencyRepository.Object,
            mockUserCurrencyRepository.Object);
        var command = new RemoveFavoriteCurrencyCommand { UserId = 1, CurrencyName = "USD" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Валюта не в избранном", result.ErrorMessage);

        mockUserCurrencyRepository.Verify(r => r.RemoveAsync(It.IsAny<UserCurrency>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenMultipleUsersHaveSameCurrency_RemovesOnlyForTargetUser()
    {
        // Arrange
        var currency = new Currency { Id = 1, Name = "USD", Rate = 75.5m };
        var user1Currency = new UserCurrency { UserId = 1, CurrencyId = 1 };

        var mockCurrencyRepository = new Mock<ICurrencyRepository>();
        mockCurrencyRepository.Setup(r => r.GetByNameAsync("USD", It.IsAny<CancellationToken>()))
            .ReturnsAsync(currency);

        var mockUserCurrencyRepository = new Mock<IUserCurrencyRepository>();
        mockUserCurrencyRepository.Setup(r => r.GetByUserAndCurrencyAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user1Currency);

        var handler = new RemoveFavoriteCurrencyCommandHandler(
            mockCurrencyRepository.Object,
            mockUserCurrencyRepository.Object);
        var command = new RemoveFavoriteCurrencyCommand { UserId = 1, CurrencyName = "USD" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorMessage);

        mockUserCurrencyRepository.Verify(r => r.RemoveAsync(
            It.Is<UserCurrency>(uc => uc.UserId == 1 && uc.CurrencyId == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WhenCurrencyNameIsNullOrWhiteSpace_ReturnsError(string? currencyName)
    {
        // Arrange
        var mockCurrencyRepository = new Mock<ICurrencyRepository>();
        mockCurrencyRepository.Setup(r => r.GetByNameAsync("", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Currency?)null);
        mockCurrencyRepository.Setup(r => r.GetByNameAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Currency?)null);

        var mockUserCurrencyRepository = new Mock<IUserCurrencyRepository>();

        var handler = new RemoveFavoriteCurrencyCommandHandler(
            mockCurrencyRepository.Object,
            mockUserCurrencyRepository.Object);
        var command = new RemoveFavoriteCurrencyCommand { UserId = 1, CurrencyName = currencyName! };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Валюта не найдена", result.ErrorMessage);

        if (currencyName == null)
        {
            mockCurrencyRepository.Verify(r => r.GetByNameAsync(null, It.IsAny<CancellationToken>()), Times.Once);
        }
        else
        {
            mockCurrencyRepository.Verify(r => r.GetByNameAsync(currencyName, It.IsAny<CancellationToken>()), Times.Once);
        }
        mockUserCurrencyRepository.Verify(r => r.RemoveAsync(It.IsAny<UserCurrency>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public async Task Handle_WhenUserIdIsInvalid_DoesNotRemoveFromDatabase(int userId)
    {
        // Arrange
        var currency = new Currency { Id = 1, Name = "USD", Rate = 75.5m };
        var userCurrency = new UserCurrency { UserId = userId, CurrencyId = 1 };

        var mockCurrencyRepository = new Mock<ICurrencyRepository>();
        mockCurrencyRepository.Setup(r => r.GetByNameAsync("USD", It.IsAny<CancellationToken>()))
            .ReturnsAsync(currency);

        var mockUserCurrencyRepository = new Mock<IUserCurrencyRepository>();
        mockUserCurrencyRepository.Setup(r => r.GetByUserAndCurrencyAsync(userId, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userCurrency);

        var handler = new RemoveFavoriteCurrencyCommandHandler(
            mockCurrencyRepository.Object,
            mockUserCurrencyRepository.Object);
        var command = new RemoveFavoriteCurrencyCommand { UserId = userId, CurrencyName = "USD" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorMessage);

        mockUserCurrencyRepository.Verify(r => r.RemoveAsync(
            It.Is<UserCurrency>(uc => uc.UserId == userId && uc.CurrencyId == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
