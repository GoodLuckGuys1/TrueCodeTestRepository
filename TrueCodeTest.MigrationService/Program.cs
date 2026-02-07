using Microsoft.EntityFrameworkCore;
using TrueCodeTest.Shared.Domain.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure PostgreSQL connection
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

// app.UseHttpsRedirection(); // Disabled for HTTP-only service

// Migration endpoint
app.MapPost("/migrate", async (ApplicationDbContext dbContext) =>
{
    try
    {
        await dbContext.Database.MigrateAsync();
        return Results.Ok(new { message = "Database migrated successfully" });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Migration failed: {ex.Message}");
    }
})
.WithName("MigrateDatabase")
.WithOpenApi();

app.Run();
