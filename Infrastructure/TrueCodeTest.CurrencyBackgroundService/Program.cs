using Microsoft.EntityFrameworkCore;
using System.Text;
using TrueCodeTest.CurrencyBackgroundService;
using TrueCodeTest.Shared.Domain.Data;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Port=5432;Database=TrueCodeTestDb;Username=postgres;Password=postgres";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.Configure<CurrencyUpdateOptions>(
    builder.Configuration.GetSection("CurrencyUpdate"));

builder.Services.AddHttpClient();
builder.Services.AddHostedService<CurrencyUpdateWorker>();

// регистрируем кодировки, для корректной обработки ответа от ЦБ
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var host = builder.Build();
host.Run();
