using Microsoft.AspNetCore.Mvc;
using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using StudentPayments_API.DTOs.Responses;
namespace StudentPayments_API.Controllers;

[Authorize] //Requires an authenticated user to access the endpoints in this controller
[ApiController] 
[Route("api/[controller]")] //Defines the base route

public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService; //Private field to hold the payment service
    private readonly ILogger<PaymentsController> _logger; //Private field to hold the logger
    public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }
    [HttpPost("register")]
    public async Task<IActionResult> RegisterPayment([FromBody] PaymentDto dto)
    {
        var (success, message) = await _paymentService.RegisterPaymentAsync(dto);
        if (!success)
        {
            if(message == "Student not registered")
            {
                return NotFound(new
                {
                    message
                });
            }else if(message == "Invalid payment type." || message == "Invalid payment channel.")
            {
                return BadRequest(new
                {
                    message
                });
            }
            else
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while registering the payment."
                });
            }
        }
        return Ok(new
        {
            message
        });
    }
    [HttpGet("my-payments")]
    public async Task<IActionResult> GetMyPayments()
    {
        try
        {
            var studentIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (studentIdClaim == null || !int.TryParse(studentIdClaim.Value, out var studentId))
            {
                return Unauthorized(new
                {
                    message = "Invalid token: Student ID claim is missing or invalid."
                });
            }
            var payments = await _paymentService.GetPaymentForStudentAsync(studentId);
            var result = payments.Select(p => new PaymentResponseDto
            {
                StudentId = p.StudentId,
                ReferenceNumber = p.ReferenceNumber,
                PaymentDateTime = p.PaymentDateTime,
                PaymentType = p.PaymentType.ToString(),
                PaymentChannel = p.PaymentChannel.ToString(),
                Amount = p.Amount
            }).ToList();
            if(result.Count == 0)
            {
                return NotFound(new
                {
                    message = "No payments found for the student."
                });
            }
            return Ok(new
            {
                payments = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving payments for student.");
            return StatusCode(500, new
            {
                message = "An error occurred while retrieving payments."
            });
        }
    }
}