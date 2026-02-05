using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.DTOs.Responses;
namespace StudentPayments_API.Services.Interfaces;


public interface IStudentValidationService
//Define an interface for student validation services
{
    //Declare a method for validating a student asynchronously
    Task<StudentValidationResponseDto> ValidateStudentAsync(StudentValidationRequestDto dto);
}