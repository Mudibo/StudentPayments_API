using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.DTOs.Responses;
namespace StudentPayments_API.Services.Interfaces;


public interface IStudentValidationService
//Define an interface for student validation services
{
    Task<StudentValidationResponseDto> ValidateStudentAsync(StudentValidationRequestDto dto);
}
