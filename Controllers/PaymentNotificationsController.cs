using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.DTOs.Responses;
using StudentPayments_API.Models.Enums;
using StudentPayments_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace StudentPayments_API.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentNotificationsController : ControllerBase
{
    private readonly IPaymentNotificationService _paymentNotificationService;
    private readonly ILogger<PaymentNotificationsController> _logger;

    public PaymentNotificationsController(IPaymentNotificationService paymentNotificationService, ILogger<PaymentNotificationsController> logger)
    {
        _paymentNotificationService = paymentNotificationService;
        _logger = logger;
    }
    [Authorize(Policy = "PaymentNotification")]
    [HttpPost("notification")]
    public async Task<IActionResult> Notify(
        [FromBody] PaymentNotificationRequestDto dto,
        [FromHeader(Name = "Idempotency-Key")] string idempotencyKey
    )
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(new ApiErrorDto
            {
                error = "Validation failed",
                error_description = string.Join("; ", errors)
            });
        }
        //Extract clientId from token
        var clientId = User.FindFirst("client_id")?.Value;
        if (string.IsNullOrEmpty(clientId))
        {
            _logger.LogWarning("Missing client_id claim in token.");
            return Unauthorized(new ApiErrorDto
            {
                error = OAuthErrorEnum.InvalidClient.ToOAuthErrorString(),
                error_description = "The token provided is not valid"
            });
        }
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return BadRequest(new ApiErrorDto
            {
                error = OAuthErrorEnum.InvalidRequest.ToOAuthErrorString(),
                error_description = "Missing Idempotency-Key header."
            });
        }
        try
        {
            var result = await _paymentNotificationService.ProcessNotificationAsync(dto, idempotencyKey, clientId);
            if (result.Success)
            {
                return Ok(result);
            }
            else if (result.Error == OAuthErrorEnum.NotFound.ToOAuthErrorString())
            {
                return NotFound(result);
            }
            else if (result.Error == OAuthErrorEnum.InvalidClient.ToOAuthErrorString())
            {
                return Unauthorized(result);
            }
            else if (result.Error == OAuthErrorEnum.Unauthorized.ToOAuthErrorString())
            {
                return Unauthorized(result);
            }
            else if (result.Error == OAuthErrorEnum.Conflict.ToOAuthErrorString())
            {
                return Conflict(result);
            }
            else
            {
                return StatusCode(500, result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment notification.");
            return StatusCode(500, new
            {
                error = "An error occurred while processing the payment notification."
            });
        }
    }

    [Authorize(Policy = "PaymentNotification")]
    [HttpGet("{admissionNumber}/notifications")]
    public async Task<IActionResult> GetStudentPayments(
        [FromRoute] string admissionNumber,
        [FromQuery] int page,
        [FromQuery] int pageSize
    )
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(new ApiErrorDto
            {
                error = "Validation failed",
                error_description = string.Join("; ", errors)
            });
        }
        //Extract clientId from token
        var clientId = User.FindFirst("client_id")?.Value;
        if (string.IsNullOrEmpty(clientId))
        {
            _logger.LogWarning("Missing client_id claim in token.");
            return Unauthorized(new ApiErrorDto
            {
                error = OAuthErrorEnum.InvalidClient.ToOAuthErrorString(),
                error_description = "The token provided is not valid"
            });
        }

        //Enforce max page size
        if (pageSize > 20) pageSize = 20;
        if (page < 1) page = 1;

        var dto = new GetStudentPaymentsRequestDto
        {
            AdmissionNumber = admissionNumber,
            Page = page,
            PageSize = pageSize
        };
        try
        {
            var result = await _paymentNotificationService.GetStudentPaymentNotificationsAsync(dto);
            if (result.TotalCount > 0)
            {
                return Ok(result);
            }
            else
            {
                return NotFound(new ApiErrorDto
                {
                    error = OAuthErrorEnum.NotFound.ToOAuthErrorString(),
                    error_description = "No payment notifications found for student: " + admissionNumber
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving student payments. ExceptionType: {ExceptionType}, StackTrace: {StackTrace}", ex.GetType().Name, ex.StackTrace);
            return StatusCode(500, new ApiErrorDto
            {
                error = OAuthErrorEnum.ServerError.ToOAuthErrorString(),
                error_description = ex.Message
            });
        }
    }
}