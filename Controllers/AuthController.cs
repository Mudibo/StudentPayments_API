using Microsoft.AspNetCore.Mvc;
using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.Data;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using StudentPayments_API.Models;

namespace StudentPayments_API.Controllers;

[ApiController] //Inform ASP.NET Core that this is an API controller
[Route("api/[controller]")] //Base route for this controller (/api/Auth)
public class AuthController : ControllerBase
{
    private readonly StudentPaymentsDbContext _context; //Hold a reference to the db
    private readonly IConfiguration _config;

    public AuthController(StudentPaymentsDbContext context, IConfiguration config)
    {
        _context = context; //Inject the database context
        _config = config; //Inject the configuration system which are saved in the private fields
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

        //Generate JWT token
        var tokenHandler = new JwtSecurityTokenHandler(); //Create JWT
        var key = Encoding.UTF8.GetBytes(_config["Jwt:Secret"]);
        var expires = DateTime.UtcNow.AddMinutes(30); //Token expiration time
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("admissionNumber", student.AdmissionNumber),
                new Claim("program", student.Program.ToString()),
                new Claim("mobileNumber", student.MobileNumber)
            }),
            Expires = expires,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor); //Create the token

        return Ok(new
        {
            token = tokenHandler.WriteToken(token),
            expiration = expires
        });
    }
}