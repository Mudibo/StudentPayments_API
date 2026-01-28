using Microsoft.AspNetCore.Mvc;
using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
namespace StudentPayments_API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]

public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
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
}