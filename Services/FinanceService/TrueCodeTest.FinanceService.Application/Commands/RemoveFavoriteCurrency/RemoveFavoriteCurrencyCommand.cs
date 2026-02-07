using MediatR;

namespace TrueCodeTest.FinanceService.Application.Commands.RemoveFavoriteCurrency;

public record RemoveFavoriteCurrencyCommand : IRequest<RemoveFavoriteCurrencyResult>
{
    public int UserId { get; init; }
    public string CurrencyName { get; init; } = string.Empty;
}
