using MediatR;
using TrueCodeTest.Shared.Domain.Entities;

namespace TrueCodeTest.FinanceService.Application.Queries;

public class GetUserCurrenciesQuery : IRequest<GetUserCurrenciesResult>
{
    public int UserId { get; set; }
}

public class GetUserCurrenciesResult
{
    public bool Success { get; set; }
    public List<CurrencyDto>? Currencies { get; set; }
    public string? ErrorMessage { get; set; }
}

public class CurrencyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
}
