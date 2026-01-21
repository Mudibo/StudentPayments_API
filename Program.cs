
using Microsoft.EntityFrameworkCore;

foreach (System.Collections.DictionaryEntry de in Environment.GetEnvironmentVariables())
{
    Console.WriteLine($"{de.Key} = {de.Value}");
}

var builder = WebApplication.CreateBuilder(args);

// Print the connection string for debugging
Console.WriteLine("Loaded connection string: " + builder.Configuration.GetConnectionString("DefaultConnection"));

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<StudentPaymentsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))); //Inform ASP.NET Core to use connection string and PostgreSQL provider for the DbContext
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");


// Temporary endpoint to test database connection
app.MapGet("/dbtest", async (StudentPaymentsDbContext db) =>
{
    try
    {
        var count = await db.Students.CountAsync();
        return Results.Ok(new { message = "Database connection successful!", studentCount = count });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database connection failed: {ex.Message}");
    }
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
