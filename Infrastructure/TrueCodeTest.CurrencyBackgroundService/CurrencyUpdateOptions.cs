namespace TrueCodeTest.CurrencyBackgroundService;

public class CurrencyUpdateOptions
{
    public string CbrUrl { get; set; } = string.Empty;
    public int UpdateIntervalHours { get; set; } = 24;
}
