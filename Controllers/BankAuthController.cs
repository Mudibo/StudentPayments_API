using Microsoft.AspNetCore.Mvc;
using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.Services.Interfaces;
using StudentPayments_API.Models.Enums;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]

public class BankAuthController : ControllerBase
{
    private readonly IBankClientService _bankClientService;
    public BankAuthController(IBankClientService bankClientService)
    {
        _bankClientService = bankClientService;
    }
    [HttpPost("authenticate")]
    public async Task<IActionResult> AuthenticateBankClient([FromBody] BankClientAuthRequestDto dto)
    {
        var response = await _bankClientService.AuthenticateBankClientAsync(dto);
        if (!response.Success)
        {
            if(response.ErrorEnum == BankAuthErrorEnum.InvalidCredentials || response.ErrorEnum == BankAuthErrorEnum.ClientInactive)
            {
                return Unauthorized(new
                {
                    message = response.Message
                });
            }
            if(response.ErrorEnum == BankAuthErrorEnum.DatabaseError || response.ErrorEnum == BankAuthErrorEnum.TransientError)
            {
                return StatusCode(503, new
                {
                    message = "Database error occurred. Please try again."
                });
            }
            return StatusCode(500, new
            {
                message = "An unexpected error occurred. Please try again"
            });
        }
        return Ok(response);
    }
}