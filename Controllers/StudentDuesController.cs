using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.Services.Interfaces;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]

public class StudentDuesController : ControllerBase
{
    private readonly IStudentDuesService _studentDuesService;
    public StudentDuesController(IStudentDuesService studentDuesService)
    {
        _studentDuesService = studentDuesService;
    }
    [Authorize(Roles ="Admin")]
    [HttpPost]
    public async Task<IActionResult>AddStudentDues([FromBody] AddStudentDuesDto dto)
    {
        var response = await _studentDuesService.AddDuesAsync(dto);
        if(!response.Success)
            return BadRequest(response.Message);
        
        return Ok(response);
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