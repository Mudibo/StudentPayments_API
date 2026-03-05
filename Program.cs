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
using StudentPayments_API.Security.OAuthScopes;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using AspNetCoreRateLimit.Redis;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog.Sinks.Async;
using AspNetCoreRateLimit.Redis;
using StudentPayments_API.Models.Enums;
// Register the enum mapping globally for Npgsql




foreach (System.Collections.DictionaryEntry de in Environment.GetEnvironmentVariables())
{
    Console.WriteLine($"{de.Key} = {de.Value}");
}

Log.Logger = new LoggerConfiguration()
    .WriteTo.Async(a => a.Console())
    .WriteTo.Async(a => a.File("Logs/log-.txt", rollingInterval: RollingInterval.Day))
    .CreateLogger();


var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// Print the connection string for debugging
Console.WriteLine("Loaded connection string: " + builder.Configuration.GetConnectionString("DefaultConnection"));

builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "StudentPayments API", Version = "v1" });
    // JWT Bearer for most endpoints
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    // Basic Auth for OAuth token endpoint
    options.AddSecurityDefinition("Basic", new OpenApiSecurityScheme
    {
        Description = "Basic Authorization header for client_id:client_secret. Example: 'Basic Base64(client_id:client_secret)'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "basic"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
    options.OperationFilter<StudentPayments_API.Swagger.IdempotencyKeyHeaderOperationFilter>();
    // Register BasicAuthOperationFilter for OAuth token endpoint
    options.OperationFilter<StudentPayments_API.Swagger.BasicAuthOperationFilter>();
});



builder.Services.AddScoped<IStudentDuesService, StudentDuesService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddScoped<IStudentRegistrationService, StudentRegistrationService>();
builder.Services.AddScoped<IStudentValidationService, StudentValidationService>();
builder.Services.AddScoped<IAuthService, AuthService>();
/*builder.Services.AddScoped<IPaymentIntentService, PaymentIntentService>(); */

// Register PaymentNotificationService for DI
builder.Services.AddScoped<IPaymentNotificationService, PaymentNotificationService>();

// Register Npgsql enum mapping using NpgsqlDataSourceBuilder (Npgsql 7+/8+)
var dataSourceBuilder = new Npgsql.NpgsqlDataSourceBuilder(builder.Configuration.GetConnectionString("DefaultConnection"));

// Map all PostgreSQL enums to C# enums
dataSourceBuilder.MapEnum<StudentPayments_API.Models.Enums.EnrollmentStatusEnum>();
dataSourceBuilder.MapEnum<StudentPayments_API.Models.Enums.ProgramEnum>();
dataSourceBuilder.MapEnum<StudentPayments_API.Models.Enums.PaymentTypeEnum>();
dataSourceBuilder.MapEnum<StudentPayments_API.Models.Enums.PaymentChannelEnum>();
dataSourceBuilder.MapEnum<StudentPayments_API.Models.Enums.IdempotencyResourceTypeEnum>();
dataSourceBuilder.MapEnum<StudentPayments_API.Models.Enums.PaymentTransactionStatusEnum>();
dataSourceBuilder.MapEnum<StudentPayments_API.Models.Enums.CurrencyEnum>();
var dataSource = dataSourceBuilder.Build();
builder.Services.AddSingleton(dataSource);
builder.Services.AddDbContext<StudentPaymentsDbContext>(options =>
    options.UseNpgsql(dataSource, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5, // Number of retries
            maxRetryDelay: TimeSpan.FromSeconds(10), // Delay between retries
            errorCodesToAdd: null //default transient errors
        );
        npgsqlOptions.CommandTimeout(120); // Set command timeout to 120 seconds
    })
);
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
//Only allow access to student validation endpoint if the token has the correct scope claim
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("StudentValidation", policy => policy.RequireClaim("scope", OAuthScopeEnum.StudentValidate));
    options.AddPolicy("PaymentNotification", policy => policy.RequireClaim("scope", OAuthScopeEnum.PaymentNotification));
});


//Register the student validation service
builder.Services.AddScoped<StudentPayments_API.Services.Interfaces.IStudentValidationService, StudentPayments_API.Services.Implementations.StudentValidationService>();
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
// Register the Redis connection multiplexer for AspNetCoreRateLimit.Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis"))
);
builder.Services.AddRedisRateLimiting();

//Redis Cache Configuration
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});


var app = builder.Build();

// Trust forwarded headers from nginx
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    
});

//app.UseIpRateLimiting();
app.UseAuthentication();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthorization();
app.MapControllers();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.Run();
