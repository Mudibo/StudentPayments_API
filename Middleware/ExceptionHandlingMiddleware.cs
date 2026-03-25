using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Net;
using System.Threading.Tasks;

namespace StudentPayments_API.Middleware;
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (InvalidOperationException ex) when (
            ex.InnerException is Npgsql.NpgsqlException npgsqlEx &&
            (npgsqlEx.InnerException is TimeoutException || npgsqlEx.IsTransient)
        )
        {
            _logger.LogError(ex, "Database timeout or transient error (wrapped in InvalidOperationException).");
            context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
            await context.Response.WriteAsJsonAsync(new { error = "Database timeout or transient error. Please try again later." });
        }
        catch (InvalidOperationException ex) when (
            ex.InnerException is TimeoutException
        )
        {
            _logger.LogError(ex, "Timeout occurred (wrapped in InvalidOperationException).");
            context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
            await context.Response.WriteAsJsonAsync(new { error = "Timeout occurred. Please try again later." });
        }
        catch (NpgsqlException ex) when (ex.InnerException is TimeoutException || ex.IsTransient)
        {
            _logger.LogError(ex, "Database timeout or transient error.");
            context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
            await context.Response.WriteAsJsonAsync(new { error = "Database timeout or transient error. Please try again later." });
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Timeout occurred.");
            context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
            await context.Response.WriteAsJsonAsync(new { error = "Timeout occurred. Please try again later." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred.");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred." });
        }
    }
}