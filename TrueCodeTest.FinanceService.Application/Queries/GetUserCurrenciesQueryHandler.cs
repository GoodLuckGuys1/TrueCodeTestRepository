using MediatR;
using Microsoft.Extensions.Logging;
using TrueCodeTest.FinanceService.Application.Queries;
using TrueCodeTest.FinanceService.Domain.Interfaces;

namespace TrueCodeTest.FinanceService.Application.Queries;

public class GetUserCurrenciesQueryHandler : IRequestHandler<GetUserCurrenciesQuery, GetUserCurrenciesResult>
{
    private readonly ICurrencyRepository _currencyRepository;
    private readonly ILogger<GetUserCurrenciesQueryHandler> _logger;

    public GetUserCurrenciesQueryHandler(
        ICurrencyRepository currencyRepository,
        ILogger<GetUserCurrenciesQueryHandler> logger)
    {
        _currencyRepository = currencyRepository;
        _logger = logger;
    }

    public async Task<GetUserCurrenciesResult> Handle(GetUserCurrenciesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var currencies = await _currencyRepository.GetUserFavoriteCurrenciesAsync(request.UserId, cancellationToken);

            var currencyDtos = currencies.Select(c => new CurrencyDto
            {
                Id = c.Id,
                Name = c.Name,
                Rate = c.Rate
            }).ToList();

            return new GetUserCurrenciesResult
            {
                Success = true,
                Currencies = currencyDtos
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user currencies");
            return new GetUserCurrenciesResult
            {
                Success = false,
                ErrorMessage = "An error occurred while retrieving currency rates"
            };
        }
    }
}
