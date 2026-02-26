using System.Threading.Tasks;
using StudentPayments_API.Models;
using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.DTOs.Responses;

namespace StudentPayments_API.Services.Interfaces;

//An interface to declare the contract for registration logic
public interface IStudentRegistrationService
{
    Task<StudentRegistrationResponseDto> RegisterStudentAsync(StudentRegistrationDto dto);
}