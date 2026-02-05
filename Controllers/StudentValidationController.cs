using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPayments_API.Data;
using StudentPayments_API.Services.Interfaces;
using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.DTOs.Responses;

namespace StudentPayments_API.Controllers;

//Creating a controller to handle student validation endpoint and require JWT authentication

[Authorize(Roles = "BankClient")] //Require JWT authentication for all actions in this controller
[ApiController] 
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
    [HttpPost("validate")]
    //async as the validation service performs asynchronous work (Database Access)
    public async Task<IActionResult> ValidateStudent([FromBody] StudentValidationRequestDto dto)
    {
        if(string.IsNullOrEmpty(dto.AdmissionNumber)){
            return BadRequest(new ApiErrorDto
            {
                Message = "Admission number is required",
            });
        }
        var response = await _validationService.ValidateStudentAsync(dto);
        switch(response.Status){
            case Models.StudentValidationStatus.Valid:
                return Ok(response);
            case Models.StudentValidationStatus.NotFound:
                return NotFound(new ApiErrorDto {
                    Message = response.Message
                });
            case Models.StudentValidationStatus.Inactive:
                return StatusCode(403, new ApiErrorDto {
                    Message = response.Message
                });
            default:
                return StatusCode(500, new ApiErrorDto
                {
                    Message = "An unexpected error occurred during student validation"
                });
        }
    }
}