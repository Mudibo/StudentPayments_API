using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.DTOs.Responses;
using System.Threading.Tasks;

namespace StudentPayments_API.Services.Interfaces;

public interface IStudentService
{
    Task<PaginatedResultDto<StudentDetailsSummaryResponseDto>> GetStudentsAsync(GetStudentsRequestDto dto);
}