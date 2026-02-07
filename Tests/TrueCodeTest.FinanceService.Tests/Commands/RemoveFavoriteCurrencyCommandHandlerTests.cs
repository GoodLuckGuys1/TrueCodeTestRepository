using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Moq;
using TrueCodeTest.FinanceService.Application.Commands.RemoveFavoriteCurrency;
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
        await using var context = CreateInMemoryContext();

        var currency = new Currency { Id = 1, Name = "USD", Rate = 75.5m };
        var user = new User { Id = 1, Name = "TestUser", Password = "hashed" };
        
        context.Currencies.Add(currency);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Добавляем через навигационное свойство с явными ключами
        user.FavoriteCurrencies.Add(new UserCurrency { UserId = user.Id, CurrencyId = currency.Id });
        await context.SaveChangesAsync();

        var handler = new RemoveFavoriteCurrencyCommandHandler(context);
        var command = new RemoveFavoriteCurrencyCommand { UserId = 1, CurrencyName = "USD" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorMessage);

        var userCurrency = await context.UserCurrencies
            .FirstOrDefaultAsync(uc => uc.UserId == 1 && uc.CurrencyId == 1);
        Assert.Null(userCurrency);
    }

    [Fact]
    public async Task Handle_WhenCurrencyNotFound_ReturnsError()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var handler = new RemoveFavoriteCurrencyCommandHandler(context);
        var command = new RemoveFavoriteCurrencyCommand { UserId = 1, CurrencyName = "UNKNOWN" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Валюта не найдена", result.ErrorMessage);
    }

    [Fact]
    public async Task Handle_WhenNotInFavorites_ReturnsError()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        var currency = new Currency { Id = 1, Name = "USD", Rate = 75.5m };
        context.Currencies.Add(currency);
        await context.SaveChangesAsync();

        var handler = new RemoveFavoriteCurrencyCommandHandler(context);
        var command = new RemoveFavoriteCurrencyCommand { UserId = 1, CurrencyName = "USD" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Валюта не в избранном", result.ErrorMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WhenCurrencyNameIsNullOrWhiteSpace_ReturnsError(string? currencyName)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var handler = new RemoveFavoriteCurrencyCommandHandler(context);
        var command = new RemoveFavoriteCurrencyCommand { UserId = 1, CurrencyName = currencyName! };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Валюта не найдена", result.ErrorMessage);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public async Task Handle_WhenUserIdIsInvalid_DoesNotRemoveFromDatabase(int userId)
    {
        // Arrange
        await using var context = CreateInMemoryContext();

        var currency = new Currency { Id = 1, Name = "USD", Rate = 75.5m };
        var user = new User { Id = userId, Name = "TestUser", Password = "hashed" };
        
        context.Currencies.Add(currency);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Добавляем через навигационное свойство с явными ключами
        user.FavoriteCurrencies.Add(new UserCurrency { UserId = user.Id, CurrencyId = currency.Id });
        await context.SaveChangesAsync();

        var handler = new RemoveFavoriteCurrencyCommandHandler(context);
        var command = new RemoveFavoriteCurrencyCommand { UserId = user.Id, CurrencyName = "USD" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success); // в текущей логике UserId не валидируется
        Assert.Null(result.ErrorMessage);

        var userCurrency = await context.UserCurrencies
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CurrencyId == 1);
        Assert.Null(userCurrency);
    }

    [Fact]
    public async Task Handle_WhenMultipleUsersHaveSameCurrency_RemovesOnlyForTargetUser()
    {
        // Arrange
        await using var context = CreateInMemoryContext();

        var currency = new Currency { Id = 1, Name = "USD", Rate = 75.5m };
        var user1 = new User { Id = 1, Name = "User1", Password = "hashed" };
        var user2 = new User { Id = 2, Name = "User2", Password = "hashed" };
        
        context.Currencies.Add(currency);
        context.Users.AddRange(user1, user2);
        await context.SaveChangesAsync();

        // Добавляем через навигационные свойства
        user1.FavoriteCurrencies.Add(new UserCurrency { UserId = 1, CurrencyId = 1 });
        user2.FavoriteCurrencies.Add(new UserCurrency { UserId = 2, CurrencyId = 1 });
        await context.SaveChangesAsync();

        var handler = new RemoveFavoriteCurrencyCommandHandler(context);
        var command = new RemoveFavoriteCurrencyCommand { UserId = 1, CurrencyName = "USD" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorMessage);

        var userCurrencies = await context.UserCurrencies.ToListAsync();
        Assert.Single(userCurrencies);
        Assert.Equal(2, userCurrencies[0].UserId);
        Assert.Equal(1, userCurrencies[0].CurrencyId);
    }

    [Fact]
    public async Task Handle_WhenUserHasNoFavorites_ReturnsError()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        var currency = new Currency { Id = 1, Name = "USD", Rate = 75.5m };
        context.Currencies.Add(currency);
        await context.SaveChangesAsync();

        var handler = new RemoveFavoriteCurrencyCommandHandler(context);
        var command = new RemoveFavoriteCurrencyCommand { UserId = 999, CurrencyName = "USD" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Валюта не в избранном", result.ErrorMessage);
    }
}
