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
using Serilog;
using AspNetCoreRateLimit;
using StudentPayments_API.Middleware;
// Register the enum mapping globally for Npgsql




foreach (System.Collections.DictionaryEntry de in Environment.GetEnvironmentVariables())
{
    Console.WriteLine($"{de.Key} = {de.Value}");
}

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();


var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});
// Print the connection string for debugging
Console.WriteLine("Loaded connection string: " + builder.Configuration.GetConnectionString("DefaultConnection"));

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// Removed unsupported AddOpenApi for .NET 8.0
builder.Services.AddScoped<IStudentDuesService, StudentDuesService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddScoped<IStudentRegistrationService, StudentRegistrationService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IStudentValidationService, StudentValidationService>();

// Register Npgsql enum mapping using NpgsqlDataSourceBuilder (Npgsql 7+/8+)
var dataSourceBuilder = new Npgsql.NpgsqlDataSourceBuilder(builder.Configuration.GetConnectionString("DefaultConnection"));

// Map all PostgreSQL enums to C# enums
dataSourceBuilder.MapEnum<StudentPayments_API.Models.EnrollmentStatusEnum>("public.enrollment_enum");
dataSourceBuilder.MapEnum<StudentPayments_API.Models.ProgramEnum>("public.program_enum");
dataSourceBuilder.MapEnum<StudentPayments_API.Models.PaymentTypeEnum>("public.payment_type_enum");
dataSourceBuilder.MapEnum<StudentPayments_API.Models.PaymentChannelEnum>("public.payment_channel_enum");
var dataSource = dataSourceBuilder.Build();
builder.Services.AddSingleton(dataSource);
builder.Services.AddDbContext<StudentPaymentsDbContext>(options =>
    options.UseNpgsql(dataSource));
builder.Services.AddScoped<IBankClientService, BankClientService>();

//Configure token validation
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"])),
            ValidateLifetime = true
        };
    });
builder.Services.AddAuthorization();

//Register the student validation service
builder.Services.AddScoped<StudentPayments_API.Services.Interfaces.IStudentValidationService, StudentPayments_API.Services.Implementations.StudentValidationService>();
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddInMemoryRateLimiting();
var app = builder.Build();

app.UseAuthentication();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthorization();
app.UseIpRateLimiting();
app.MapControllers();
// Configure the HTTP request pipeline.
// Removed unsupported MapOpenApi for .NET 8.0

app.UseHttpsRedirection();
app.Run();

