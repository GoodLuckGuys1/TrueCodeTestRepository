using Microsoft.EntityFrameworkCore;
using TrueCodeTest.FinanceService.Application.Commands.AddFavoriteCurrency;
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

        var handler = new AddFavoriteCurrencyCommandHandler(context);
        var command = new AddFavoriteCurrencyCommand { UserId = 1, CurrencyName = "USD" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorMessage);

        var userCurrency = await context.UserCurrencies
            .FirstOrDefaultAsync(uc => uc.UserId == 1 && uc.CurrencyId == 1);
        Assert.NotNull(userCurrency);
        Assert.Equal(1, userCurrency.UserId);
        Assert.Equal(1, userCurrency.CurrencyId);
    }

    [Fact]
    public async Task Handle_WhenCurrencyNotFound_ReturnsError()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var handler = new AddFavoriteCurrencyCommandHandler(context);
        var command = new AddFavoriteCurrencyCommand { UserId = 1, CurrencyName = "UNKNOWN" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Валюта не найдена", result.ErrorMessage);
    }

    [Fact]
    public async Task Handle_WhenAlreadyInFavorites_ReturnsError()
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

        var handler = new AddFavoriteCurrencyCommandHandler(context);
        var command = new AddFavoriteCurrencyCommand { UserId = 1, CurrencyName = "USD" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Валюта уже в избранном", result.ErrorMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WhenCurrencyNameIsNullOrWhiteSpace_ReturnsError(string? currencyName)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var handler = new AddFavoriteCurrencyCommandHandler(context);
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
        await using var context = CreateInMemoryContext();

        var currency = new Currency { Id = 1, Name = "USD", Rate = 75.5m };
        context.Currencies.Add(currency);
        await context.SaveChangesAsync();

        var handler = new AddFavoriteCurrencyCommandHandler(context);
        var command = new AddFavoriteCurrencyCommand { UserId = userId, CurrencyName = "USD" };

        // Сбрасываем отслеживание перед вызовом хендлера
        context.ChangeTracker.Clear();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success); // в текущей логике UserId не валидируется
        Assert.Null(result.ErrorMessage);
        // Не проверяем наличие записи в базе, т.к. InMemory не может сохранить некорректный UserId
    }

    [Fact]
    public async Task Handle_WhenMultipleUsersAddSameCurrency_AddsForEachUser()
    {
        // Arrange
        await using var context = CreateInMemoryContext();

        var currency = new Currency { Id = 1, Name = "USD", Rate = 75.5m };
        var user1 = new User { Id = 1, Name = "User1", Password = "hashed" };
        var user2 = new User { Id = 2, Name = "User2", Password = "hashed" };

        context.Currencies.Add(currency);
        context.Users.AddRange(user1, user2);
        await context.SaveChangesAsync();

        var handler = new AddFavoriteCurrencyCommandHandler(context);

        // Act
        var result1 = await handler.Handle(new AddFavoriteCurrencyCommand { UserId = user1.Id, CurrencyName = "USD" },
            CancellationToken.None);
        var result2 = await handler.Handle(new AddFavoriteCurrencyCommand { UserId = user2.Id, CurrencyName = "USD" },
            CancellationToken.None);

        // Assert
        Assert.True(result1.Success);
        Assert.True(result2.Success);

        var userCurrencies = await context.UserCurrencies.ToListAsync();
        Assert.Equal(2, userCurrencies.Count);
        Assert.Contains(userCurrencies, uc => uc.UserId == user1.Id && uc.CurrencyId == currency.Id);
        Assert.Contains(userCurrencies, uc => uc.UserId == user2.Id && uc.CurrencyId == currency.Id);
    }
}