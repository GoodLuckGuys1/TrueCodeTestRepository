using Microsoft.EntityFrameworkCore;
using Moq;
using TrueCodeTest.FinanceService.Application.Commands.AddFavoriteCurrency;
using TrueCodeTest.FinanceService.Domain.Interfaces;
using TrueCodeTest.Shared.Domain.Data;
using TrueCodeTest.Shared.Domain.Entities;

namespace TrueCodeTest.FinanceService.Tests.Commands;

public class AddFavoriteCurrencyCommandHandlerTests
{
    private ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Handle_WhenCurrencyExistsAndNotInFavorites_AddsSuccessfully()
    {
        // Arrange
        await using var context = CreateInMemoryContext();

        var currency = new Currency { Id = 1, Name = "USD", Rate = 75.5m };
        context.Currencies.Add(currency);
        await context.SaveChangesAsync();

        var mockCurrencyRepository = new Mock<ICurrencyRepository>();
        mockCurrencyRepository.Setup(r => r.GetByNameAsync("USD", It.IsAny<CancellationToken>()))
            .ReturnsAsync(currency);

        var mockUserCurrencyRepository = new Mock<IUserCurrencyRepository>();
        mockUserCurrencyRepository.Setup(r => r.ExistsAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new AddFavoriteCurrencyCommandHandler(
            mockCurrencyRepository.Object,
            mockUserCurrencyRepository.Object);
        var command = new AddFavoriteCurrencyCommand { UserId = 1, CurrencyName = "USD" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorMessage);

        mockUserCurrencyRepository.Verify(r => r.AddAsync(
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

        var handler = new AddFavoriteCurrencyCommandHandler(
            mockCurrencyRepository.Object,
            mockUserCurrencyRepository.Object);
        var command = new AddFavoriteCurrencyCommand { UserId = 1, CurrencyName = "UNKNOWN" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Валюта не найдена", result.ErrorMessage);

        mockUserCurrencyRepository.Verify(r => r.AddAsync(It.IsAny<UserCurrency>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAlreadyInFavorites_ReturnsError()
    {
        // Arrange
        var currency = new Currency { Id = 1, Name = "USD", Rate = 75.5m };
        var user = new User { Id = 1, Name = "TestUser", Password = "hashed" };

        var mockCurrencyRepository = new Mock<ICurrencyRepository>();
        mockCurrencyRepository.Setup(r => r.GetByNameAsync("USD", It.IsAny<CancellationToken>()))
            .ReturnsAsync(currency);

        var mockUserCurrencyRepository = new Mock<IUserCurrencyRepository>();
        mockUserCurrencyRepository.Setup(r => r.ExistsAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new AddFavoriteCurrencyCommandHandler(
            mockCurrencyRepository.Object,
            mockUserCurrencyRepository.Object);
        var command = new AddFavoriteCurrencyCommand { UserId = 1, CurrencyName = "USD" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Валюта уже в избранном", result.ErrorMessage);

        mockUserCurrencyRepository.Verify(r => r.AddAsync(It.IsAny<UserCurrency>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WhenCurrencyNameIsNullOrWhiteSpace_ReturnsError(string? currencyName)
    {
        // Arrange
        var mockCurrencyRepository = new Mock<ICurrencyRepository>();
        mockCurrencyRepository.Setup(r => r.GetByNameAsync(currencyName ?? "", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Currency?)null);

        var mockUserCurrencyRepository = new Mock<IUserCurrencyRepository>();

        var handler = new AddFavoriteCurrencyCommandHandler(
            mockCurrencyRepository.Object,
            mockUserCurrencyRepository.Object);
        var command = new AddFavoriteCurrencyCommand { UserId = 1, CurrencyName = currencyName! };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Валюта не найдена", result.ErrorMessage);

    }

    [Theory]
    [InlineData(-1)]
    public async Task Handle_WhenUserIdIsInvalid_ReturnsSuccessWithoutValidation(int userId)
    {
        // Arrange
        var currency = new Currency { Id = 1, Name = "USD", Rate = 75.5m };

        var mockCurrencyRepository = new Mock<ICurrencyRepository>();
        mockCurrencyRepository.Setup(r => r.GetByNameAsync("USD", It.IsAny<CancellationToken>()))
            .ReturnsAsync(currency);

        var mockUserCurrencyRepository = new Mock<IUserCurrencyRepository>();
        mockUserCurrencyRepository.Setup(r => r.ExistsAsync(userId, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new AddFavoriteCurrencyCommandHandler(
            mockCurrencyRepository.Object,
            mockUserCurrencyRepository.Object);
        var command = new AddFavoriteCurrencyCommand { UserId = userId, CurrencyName = "USD" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success); // в текущей логике UserId не валидируется
        Assert.Null(result.ErrorMessage);

        mockUserCurrencyRepository.Verify(r => r.AddAsync(
            It.Is<UserCurrency>(uc => uc.UserId == userId && uc.CurrencyId == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenMultipleUsersAddSameCurrency_AddsForEachUser()
    {
        // Arrange
        var currency = new Currency { Id = 1, Name = "USD", Rate = 75.5m };
        var user1 = new User { Id = 1, Name = "User1", Password = "hashed" };
        var user2 = new User { Id = 2, Name = "User2", Password = "hashed" };

        var mockCurrencyRepository = new Mock<ICurrencyRepository>();
        mockCurrencyRepository.Setup(r => r.GetByNameAsync("USD", It.IsAny<CancellationToken>()))
            .ReturnsAsync(currency);

        var mockUserCurrencyRepository = new Mock<IUserCurrencyRepository>();
        mockUserCurrencyRepository.Setup(r => r.ExistsAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        mockUserCurrencyRepository.Setup(r => r.ExistsAsync(2, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new AddFavoriteCurrencyCommandHandler(
            mockCurrencyRepository.Object,
            mockUserCurrencyRepository.Object);

        // Act
        var result1 = await handler.Handle(new AddFavoriteCurrencyCommand { UserId = user1.Id, CurrencyName = "USD" },
            CancellationToken.None);
        var result2 = await handler.Handle(new AddFavoriteCurrencyCommand { UserId = user2.Id, CurrencyName = "USD" },
            CancellationToken.None);

        // Assert
        Assert.True(result1.Success);
        Assert.True(result2.Success);

        mockUserCurrencyRepository.Verify(r => r.AddAsync(
            It.Is<UserCurrency>(uc => uc.UserId == user1.Id && uc.CurrencyId == currency.Id),
            It.IsAny<CancellationToken>()), Times.Once);
        mockUserCurrencyRepository.Verify(r => r.AddAsync(
            It.Is<UserCurrency>(uc => uc.UserId == user2.Id && uc.CurrencyId == currency.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}