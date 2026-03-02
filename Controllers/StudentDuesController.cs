using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.DTOs.Responses;
using StudentPayments_API.Models.Enums;
using StudentPayments_API.Services.Interfaces;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]

public class StudentDuesController : ControllerBase
{
    private readonly IStudentDuesService _studentDuesService;
    private readonly ILogger<StudentDuesController> _logger;
    public StudentDuesController(IStudentDuesService studentDuesService, ILogger<StudentDuesController> logger)
    {
        _studentDuesService = studentDuesService;
        _logger = logger;
    }
    
    [Authorize]
    [HttpGet("balance")]
    public async Task<IActionResult> GetMyBalance()
    {
        var studentIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
        if(studentIdClaim == null || !int.TryParse(studentIdClaim.Value, out int studentId))
        {
            return Unauthorized(new
            {
                message = "Invalid token: Student ID Claim is missing or invalid."
            });
        }
        var balance = await _studentDuesService.GetStudentBalanceAsync(new GetStudentBalanceRequestDto { StudentId = studentId });
        return Ok(new
        {
            balance
        });
    }
}