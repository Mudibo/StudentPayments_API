using System.Threading.Tasks;
using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.DTOs.Responses;
using StudentPayments_API.Models;

namespace StudentPayments_API.Services.Interfaces;

public interface IStudentDuesService
{
    Task<AddStudentDuesResponseDto> AddDuesAsync(AddStudentDuesDto dto);
    Task<PaginatedResultDto<GetStudentsDuesResponseDto>> GetAllStudentDuesAsync(GetStudentsDuesRequestDto dto);
    Task<GetStudentBalanceResponseDto> GetStudentBalanceAsync(GetStudentBalanceRequestDto dto);
} 