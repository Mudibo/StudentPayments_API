using Npgsql;
using StudentPayments_API.Models;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StudentPayments_API.Data;
using StudentPayments_API.Services.Interfaces;
using StudentPayments_API.Services.Implementations;
using StudentPayments_API.Security.Interfaces;
using StudentPayments_API.Security.Implementations;
// Register the enum mapping globally for Npgsql




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
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddScoped<IStudentRegistrationService, StudentRegistrationService>();

// Register both enums for Npgsql mapping
builder.Services.AddDbContext<StudentPaymentsDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions
            .MapEnum<ProgramEnum>()
            .MapEnum<EnrollmentStatusEnum>("enrollment_enum")
    )
);

var secret = builder.Configuration["Jwt:Secret"];
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidateLifetime = false //disable expiration for testing
        };
    });
builder.Services.AddAuthorization();

//Register the student validation service
builder.Services.AddScoped<StudentPayments_API.Services.Interfaces.IStudentValidationService, StudentPayments_API.Services.Implementations.StudentValidationService>();
var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
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
