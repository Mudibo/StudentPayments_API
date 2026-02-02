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
}