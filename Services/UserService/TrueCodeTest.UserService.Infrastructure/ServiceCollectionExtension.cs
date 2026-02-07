using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrueCodeTest.Shared.Domain.Data;
using TrueCodeTest.UserService.Domain.Interfaces;
using TrueCodeTest.UserService.Infrastructure.Repositories;

namespace TrueCodeTest.UserService.Infrastructure;

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

        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
