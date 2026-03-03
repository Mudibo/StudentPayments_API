using Microsoft.EntityFrameworkCore;
using StudentPayments_API.Data;
using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.DTOs.Responses;
using StudentPayments_API.Models;
using StudentPayments_API.Services.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace StudentPayments_API.Services.Implementations;

public class StudentService : IStudentService
{
    private readonly StudentPaymentsDbContext _context;
    private readonly ILogger<StudentService> _logger;
    public StudentService(StudentPaymentsDbContext context, ILogger<StudentService> logger)
    {
        _context = context;
        _logger = logger;
    }
    public async Task<PaginatedResultDto<StudentDetailsSummaryResponseDto>> GetStudentsAsync(GetStudentsRequestDto dto)
    {
        try
        {
            var query =  _context.Students.AsNoTracking();
            var totalCount = await query.CountAsync();
            var students = await query
                .OrderBy(s => s.StudentId)
                .Skip((dto.Page - 1) * dto.PageSize)
                .Take(dto.PageSize)
                .Select(s => new StudentDetailsSummaryResponseDto
                {
                   AdmissionNumber = s.AdmissionNumber,
                   FirstName = s.FirstName,
                   LastName = s.LastName,
                   Email = s.Email,
                   MobileNumber = s.MobileNumber,
                   Program = s.Program.ToString(),
                   EnrollmentStatus = s.EnrollmentStatus.ToString()
                })
                .ToListAsync();
            return new PaginatedResultDto<StudentDetailsSummaryResponseDto>
            {
               TotalCount = totalCount,
               Page = dto.Page,
               PageSize = dto.PageSize,
               Items = students
            };
        }catch(Exception ex)
        {
            _logger.LogError("Error fetching students ExceptionType:{ExceptionType}, ExceptionName:{ExceptionName}, StackTrace:{StackTrace}", ex.GetType().Name, ex.Message, ex.StackTrace);
            throw;
        }
    }
}