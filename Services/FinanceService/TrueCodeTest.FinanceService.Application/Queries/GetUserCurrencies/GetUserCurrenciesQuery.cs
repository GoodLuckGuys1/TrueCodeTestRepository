using MediatR;
using TrueCodeTest.Shared.Domain.Common;

namespace TrueCodeTest.FinanceService.Application.Queries.GetUserCurrencies;

public class GetUserCurrenciesQuery : IRequest<GetUserCurrenciesResult>
{
    public int UserId { get; set; }
}

public class GetUserCurrenciesResult : ResultBase
{
    public List<CurrencyDto>? Currencies { get; set; }
}

public class CurrencyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
}
