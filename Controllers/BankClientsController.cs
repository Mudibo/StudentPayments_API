using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.Services.Interfaces;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]

public class BankClientsController : ControllerBase
{
    private readonly IBankClientService _bankClientService;
    public BankClientsController(IBankClientService bankClientService)
    {
        _bankClientService = bankClientService;
    }
    [Authorize(Roles ="Admin")] //Only users with the "Admin" role can access this endpoint
    [HttpPost]
    public async Task<IActionResult> AddBankClient([FromBody] CreateBankClientDto dto)
    {
        try
        {
            var response = await _bankClientService.CreateBankClientAsync(dto);
            if (!response.Success)
            {
                if(response.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                {
                    return Conflict(new
                    {
                        message = response.Message
                    });
                }
                else if(response.Message.Contains("database error", StringComparison.OrdinalIgnoreCase) || response.Message.Contains("unexpected error", StringComparison.OrdinalIgnoreCase))
                {
                    return StatusCode(500, new
                    {
                        message = response.Message
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        message = response.Message
                    });
                }
            }
            return Ok(response);
        }catch(Exception)
        {
            return StatusCode(500, new
            {
                message = "An unexpected error occurred while processing the request."
            });
        }
    }
}