using System.Threading.Tasks;
using StudentPayments_API.Models;
using StudentPayments_API.DTOs.Requests;

namespace StudentPayments_API.Services.Interfaces;

//An interface to declare the contract for registration logic
public interface IStudentRegistrationService
{
    Task<(bool success, string message, Student? student)> RegisterStudentAsync(StudentRegistrationDto dto);
}