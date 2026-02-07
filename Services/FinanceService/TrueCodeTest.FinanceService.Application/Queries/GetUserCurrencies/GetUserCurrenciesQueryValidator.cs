using TrueCodeTest.Shared.Domain.Validators;

namespace TrueCodeTest.FinanceService.Application.Queries.GetUserCurrencies;

public class GetUserCurrenciesQueryValidator : BaseValidator<GetUserCurrenciesQuery>
{
    public GetUserCurrenciesQueryValidator()
    {
        ValidateUserId(x => x.UserId);
    }
}
