using Microsoft.AspNetCore.Mvc;
using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.Data;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using StudentPayments_API.Models;
using StudentPayments_API.Security.Interfaces;

namespace StudentPayments_API.Controllers;

[ApiController] //Inform ASP.NET Core that this is an API controller
[Route("api/[controller]")] //Base route for this controller (/api/Auth)
public class AuthController : ControllerBase
{
    private readonly StudentPaymentsDbContext _context; //Hold a reference to the db
    private readonly ITokenService _tokenService;

    public AuthController(StudentPaymentsDbContext context, ITokenService tokenService)
    {
        _context = context; //Inject the database context
        _tokenService = tokenService; //Inject the configuration system which are saved in the private fields
    }
    [HttpPost("login")] //Define a POST endpoint (/api/Auth/login)
    public async Task<ActionResult> Login([FromBody] LoginDto dto)
    {
        var admissionNumber = dto.AdmissionNumber.Trim();
        var student = await _context.Students.FirstOrDefaultAsync(s => s.AdmissionNumber == admissionNumber); //Query Students table for a student with the provided admission number
        if (student == null)
        {
            return Unauthorized(new {
                message = "Invalid Credentials"
            });
        }
        if(student.EnrollmentStatus != EnrollmentStatusEnum.Active)
        {
            //Return Http 401 Unauthorized if enrollment status is not active
            return Unauthorized(new {
                message = "Your enrollment status is not active. Login is not permitted."
            });
        }
        if(!BCrypt.Net.BCrypt.Verify(dto.Password.Trim(), student.PasswordHash))
        {
            //Return Http 401 Unauthorized if password does not match
            return Unauthorized(new {
                message = "Invalid Credentials."
            });
        }
        var tokenResponse = _tokenService.GenerateToken(student);
        return Ok(tokenResponse);
    }
}