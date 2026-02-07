using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrueCodeTest.Shared.Domain.Data;
using TrueCodeTest.FinanceService.Domain.Interfaces;
using TrueCodeTest.FinanceService.Infrastructure.Repositories;

namespace TrueCodeTest.FinanceService.Infrastructure;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=TrueCodeTestDb;Username=postgres;Password=postgres";

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ICurrencyRepository, CurrencyRepository>();

        return services;
    }
}
