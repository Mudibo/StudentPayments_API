using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.DTOs.Responses;
using StudentPayments_API.Models.Enums;
using StudentPayments_API.Services.Interfaces;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]

public class BankClientsController : ControllerBase
{
    private readonly IBankClientService _bankClientService;
    private readonly ILogger<BankClientsController> _logger;
    public BankClientsController(IBankClientService bankClientService, ILogger<BankClientsController> logger)
    {
        _bankClientService = bankClientService;
        _logger = logger;
    }
    [Authorize(Roles ="Admin")] //Only users with the "Admin" role can access this endpoint
    [HttpPost]
    public async Task<IActionResult> AddBankClient([FromBody] CreateBankClientDto dto)
    {
        if(!ModelState.IsValid)
        {
            return BadRequest(new ApiErrorDto
            {
                error = OAuthErrorEnum.InvalidRequest.ToOAuthErrorString(),
                error_description = "Invalid request data. Please ensure all required fields are provided and valid."
            });
        }
        try
        {
            var response = await _bankClientService.CreateBankClientAsync(dto);
            if (!response.Success)
            {
                if(response.Error == OAuthErrorEnum.Conflict.ToOAuthErrorString())
                {
                    return Conflict(new ApiErrorDto
                    {
                        error = OAuthErrorEnum.Conflict.ToOAuthErrorString(),
                        error_description = response.Message
                    });
                }
                else if(response.Error == OAuthErrorEnum.TemporarilyUnavailable.ToOAuthErrorString())
                {
                    return StatusCode(503, new ApiErrorDto
                    {
                        error = OAuthErrorEnum.TemporarilyUnavailable.ToOAuthErrorString(),
                        error_description = response.Message
                    });
                }
                else
                {
                    return StatusCode(500, new ApiErrorDto
                    {
                        error = OAuthErrorEnum.ServerError.ToOAuthErrorString(),
                        error_description = response.Message
                    });
                }
            }
            return Ok(response);
        }catch(Exception)
        {
            return StatusCode(500, new ApiErrorDto
            {
                error = OAuthErrorEnum.ServerError.ToOAuthErrorString(),
                error_description = "An unexpected error occurred while processing the request."
            });
        }
    }
}