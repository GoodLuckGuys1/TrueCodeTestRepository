namespace TrueCodeTest.Shared.Domain.Entities;

public class UserCurrency
{
    public int UserId { get; set; }
    public int CurrencyId { get; set; }
    public User User { get; set; } = null!;
    public Currency Currency { get; set; } = null!;
}
