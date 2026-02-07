using Moq;
using TrueCodeTest.FinanceService.Application.Queries;
using TrueCodeTest.FinanceService.Application.Queries.GetUserCurrencies;
using TrueCodeTest.FinanceService.Domain.Interfaces;
using TrueCodeTest.Shared.Domain.Entities;
using Xunit;

namespace TrueCodeTest.FinanceService.Tests.Queries;

public class GetUserCurrenciesQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserHasFavoriteCurrencies_ReturnsCurrencies()
    {
        // Arrange
        var mockRepository = new Mock<ICurrencyRepository>();
        var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<GetUserCurrenciesQueryHandler>>();

        var currencies = new List<Currency>
        {
            new Currency { Id = 1, Name = "USD", Rate = 75.50m },
            new Currency { Id = 2, Name = "EUR", Rate = 85.20m }
        };

        mockRepository.Setup(r => r.GetUserFavoriteCurrenciesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currencies);

        var handler = new GetUserCurrenciesQueryHandler(mockRepository.Object, mockLogger.Object);
        var query = new GetUserCurrenciesQuery { UserId = 1 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Currencies);
        Assert.Equal(2, result.Currencies.Count);
        Assert.Equal("USD", result.Currencies[0].Name);
        Assert.Equal(75.50m, result.Currencies[0].Rate);
        Assert.Equal("EUR", result.Currencies[1].Name);
        Assert.Equal(85.20m, result.Currencies[1].Rate);
    }

    [Fact]
    public async Task Handle_WhenUserHasNoFavoriteCurrencies_ReturnsEmptyList()
    {
        // Arrange
        var mockRepository = new Mock<ICurrencyRepository>();
        var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<GetUserCurrenciesQueryHandler>>();

        mockRepository.Setup(r => r.GetUserFavoriteCurrenciesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Currency>());

        var handler = new GetUserCurrenciesQueryHandler(mockRepository.Object, mockLogger.Object);
        var query = new GetUserCurrenciesQuery { UserId = 1 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Currencies);
        Assert.Empty(result.Currencies);
    }
}
