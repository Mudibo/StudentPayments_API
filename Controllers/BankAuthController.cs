using Microsoft.AspNetCore.Mvc;
using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.Services.Interfaces;
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
            if(response.Message == "Invalid Client ID or inactive Bank Client." || response.Message == "Invalid Client Secret")
            {
                return Unauthorized(new
                {
                    message = response.Message
                });
            }
            return BadRequest(new
            {
                message = response.Message
            });
        }
        return Ok(response);
    }
}