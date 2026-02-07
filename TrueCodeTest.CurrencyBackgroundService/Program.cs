using Microsoft.EntityFrameworkCore;
using TrueCodeTest.CurrencyBackgroundService;
using TrueCodeTest.Shared.Domain.Data;

var builder = Host.CreateApplicationBuilder(args);

// Configure PostgreSQL connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Port=5432;Database=TrueCodeTestDb;Username=postgres;Password=postgres";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddHttpClient();
builder.Services.AddHostedService<CurrencyUpdateWorker>();

var host = builder.Build();
host.Run();
