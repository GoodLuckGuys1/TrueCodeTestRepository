using System.Xml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TrueCodeTest.Shared.Domain.Data;
using TrueCodeTest.Shared.Domain.Entities;

namespace TrueCodeTest.CurrencyBackgroundService;

public class CurrencyUpdateWorker : BackgroundService
{
    private readonly ILogger<CurrencyUpdateWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly CurrencyUpdateOptions _options;

    public CurrencyUpdateWorker(
        ILogger<CurrencyUpdateWorker> logger,
        IServiceProvider serviceProvider,
        IHttpClientFactory httpClientFactory,
        IOptions<CurrencyUpdateOptions> options)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await UpdateCurrenciesAsync(stoppingToken);

        // Пока поставим 24 часа, можно поменять в конфигурации
        using var timer = new PeriodicTimer(TimeSpan.FromHours(_options.UpdateIntervalHours));
        
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            await UpdateCurrenciesAsync(stoppingToken);
        }
    }

    private async Task UpdateCurrenciesAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Начало работы сервиса в {Time}", DateTimeOffset.Now);

            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync(_options.CbrUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            var xmlContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var currencies = ParseXmlResponse(xmlContent);

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            foreach (var currency in currencies)
            {
                var existingCurrency = await dbContext.Currencies
                    .FirstOrDefaultAsync(c => c.Name == currency.Name, cancellationToken);

                if (existingCurrency != null)
                {
                    existingCurrency.Rate = currency.Rate;
                    dbContext.Currencies.Update(existingCurrency);
                }
                else
                {
                    dbContext.Currencies.Add(currency);
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Обновлено {Count}", currencies.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка обновления!");
        }
    }

    private List<Currency> ParseXmlResponse(string xmlContent)
    {
        var currencies = new List<Currency>();
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlContent);

        var valuteNodes = xmlDoc.SelectNodes("//Valute");
        if (valuteNodes == null) return currencies;

        foreach (XmlNode valuteNode in valuteNodes)
        {
            var nameNode = valuteNode.SelectSingleNode("Name");
            var valueNode = valuteNode.SelectSingleNode("Value");
            var nominalNode = valuteNode.SelectSingleNode("Nominal");

            if (nameNode != null && valueNode != null && nominalNode != null)
            {
                var name = nameNode.InnerText.Trim();
                var valueStr = valueNode.InnerText.Trim().Replace(",", ".");
                var nominalStr = nominalNode.InnerText.Trim();

                if (decimal.TryParse(valueStr, out var value) && 
                    int.TryParse(nominalStr, out var nominal) && 
                    nominal > 0)
                {
                    var rate = value / nominal;
                    currencies.Add(new Currency
                    {
                        Name = name,
                        Rate = rate
                    });
                }
            }
        }

        return currencies;
    }
}
