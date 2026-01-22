using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPayments_API.Data;
using StudentPayments_API.Services.Interfaces;

namespace StudentPayments_API.Controllers;

//Creating a controller to handle student validation endpoint and require JWT authentication

[Authorize] //Require JWT authentication for all actions in this controller
[ApiController] //Indicate that this is an API controller
[Route("api/[controller]")]
public class StudentValidationController : ControllerBase
{
    //Delegating business logic to service
    private readonly IStudentValidationService _validationService;
    public StudentValidationController(IStudentValidationService validationService)
    {
        _validationService = validationService;
    }

    //Route produced: GET api/StudentValidation/validate
    [HttpGet("validate")]
    //async as the validation service performs asynchronous work (Database Access)
    public async Task<IActionResult> ValidateStudent()
    {
        //Extract claims from the JWT token
        //Syntax: User(authenticated user), Claims(list of claims), FirstOrDefault(Iterate through claims, and find the first claim whose Type matches admission number) ?.Value ->The claim value
        // The ?. operator has been used to handle null values gracefully, by returning null instead of throwing an exception if the claim is null
        var admissionNumber = User.Claims.FirstOrDefault(c => c.Type == "admissionNumber")?.Value;
        var program = User.Claims.FirstOrDefault(c => c.Type == "program" )?.Value;
        var mobileNumber = User.Claims.FirstOrDefault(c => c.Type == "mobileNumber")?.Value;

        //Validate required claims, business logic requires all 3 claims
        if(string.IsNullOrEmpty(admissionNumber) || string.IsNullOrEmpty(program) || string.IsNullOrEmpty(mobileNumber)){
            //HTTP response 400 Bad Request
            return BadRequest(new {
                message = "Required claims are missing in the token."
            });
        }
        //Call the validation service to validate the student
        var (isValid, student, message) = await _validationService.ValidateStudentAsync(admissionNumber, program, mobileNumber);
        if(isValid){
            //Http resonse 200 ok
            return Ok(new
            {
                message = "Student Validated successfully",
                student
            });
        }else{
            //Http response 404 not found
            return NotFound(new {
                message
            });
        }
    }
}