using MediatR;

namespace TrueCodeTest.FinanceService.Application.Commands.AddFavoriteCurrency;

public record AddFavoriteCurrencyCommand : IRequest<AddFavoriteCurrencyResult>
{
    public int UserId { get; init; }
    public string CurrencyName { get; init; } = string.Empty;
}
