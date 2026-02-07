using Microsoft.EntityFrameworkCore;
using TrueCodeTest.Shared.Domain.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Port=5432;Database=TrueCodeTestDb;Username=postgres;Password=postgres";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.MapPost("/migrate", async (ApplicationDbContext dbContext) =>
{
    try
    {
        await dbContext.Database.MigrateAsync();
        return Results.Ok(new { message = "Миграции применены успешно" });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Ошибка применения миграций: {ex.Message}");
    }
})
.WithName("MigrateDatabase")
.WithOpenApi();

app.Run();
