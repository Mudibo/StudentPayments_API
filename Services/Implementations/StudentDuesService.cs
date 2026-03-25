using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.DTOs.Responses;
using StudentPayments_API.Models;
using StudentPayments_API.Services.Interfaces;
using StudentPayments_API.Data;
using StudentPayments_API.Models.Enums;

namespace StudentPayments_API.Services.Implementations;

public class StudentDuesService : IStudentDuesService
{
    private readonly StudentPaymentsDbContext _context;
    private readonly ILogger<StudentDuesService> _logger;
    public StudentDuesService(StudentPaymentsDbContext context, ILogger<StudentDuesService> logger)
    {
        _context = context;
        _logger = logger;
    }
    public async Task<AddStudentDuesResponseDto> AddDuesAsync(AddStudentDuesDto dto)
    {
        try
        {
            var studentData = await _context.Students
                .Where(s => s.AdmissionNumber == dto.AdmissionNumber.Trim())
                .Select(s => new { s.AdmissionNumber, s.StudentId })
                .FirstOrDefaultAsync();
            if (studentData == null)
            {
                return new AddStudentDuesResponseDto
                {
                    Success = false,
                    Message = $"No student found with Admission Number: {dto.AdmissionNumber}",
                    Error = OAuthErrorEnum.NotFound.ToOAuthErrorString()
                };
            }
            var dues = new StudentDues
            {
                StudentId = studentData.StudentId,
                DuesAmount = dto.DuesAmount,
                DuesType = dto.DuesType,
                EffectiveDate = DateTime.SpecifyKind(dto.EffectiveDate, DateTimeKind.Utc),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.StudentDues.Add(dues);
            await _context.SaveChangesAsync();

            return new AddStudentDuesResponseDto
            {
                Success = true,
                Message = $"Dues added successfully for Admission Number: {dto.AdmissionNumber}"
            };
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error occurred while adding dues for AdmissionNumber: {AdmissionNumber}, ExceptionType: {ExceptionType}, StackTrace: {StackTrace}", dto.AdmissionNumber, dbEx.GetType().Name, dbEx.StackTrace);
            return new AddStudentDuesResponseDto
            {
                Success = false,
                Error = OAuthErrorEnum.TemporarilyUnavailable.ToOAuthErrorString(),
                Message = "A database error occurred while adding student dues. Please try again."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError("An unexpected error occurred while adding dues for AdmissionNumber: {AdmissionNumber}, ExceptionType: {ExceptionType}, StackTrace: {StackTrace}", dto.AdmissionNumber, ex.GetType().Name, ex.StackTrace);
            return new AddStudentDuesResponseDto
            {
                Success = false,
                Error = OAuthErrorEnum.ServerError.ToOAuthErrorString(),
                Message = $"An unexpected error occurred while adding student dues: {ex.Message}. Please try again."
            };
        }
    }
    public async Task<GetStudentBalanceResponseDto> GetStudentBalanceAsync(GetStudentBalanceRequestDto dto)
    {
        try
        {
            var studentData = await _context.Students
                .Where(s => s.AdmissionNumber == dto.AdmissionNumber.Trim())
                .Select(s => new { s.StudentId, s.AdmissionNumber })
                .FirstOrDefaultAsync();

            if (studentData == null)
            {
                return new GetStudentBalanceResponseDto
                {
                    Success = false,
                    Message = $"No student found with Admission Number: {dto.AdmissionNumber}",
                    Error = OAuthErrorEnum.NotFound.ToOAuthErrorString()
                };
            }

            var totalDues = await _context.StudentDues
                .Where(sd => sd.StudentId == studentData.StudentId)
                .SumAsync(sd => (decimal?)sd.DuesAmount) ?? 0m;

            var totalPaid = await _context.PaymentTransactions
                .Where(p => p.StudentId == studentData.StudentId)
                .SumAsync(P => (decimal?)P.Amount) ?? 0m;

            return new GetStudentBalanceResponseDto
            {
                AdmissionNumber = studentData.AdmissionNumber,
                TotalDues = totalDues,
                TotalPaid = totalPaid,
                Balance = totalDues - totalPaid,
                Success = true,
                Message = "Balance retrieved successfully."
            };
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError("Database error occurred while retrieving student balance for Admission Number: {admissionNumber}, ExceptionType: {ExceptionType}, StackTrace: {StackTrace}", dto.AdmissionNumber, dbEx.GetType().Name, dbEx.StackTrace);
            return new GetStudentBalanceResponseDto
            {
                Success = false,
                Message = "A database error occurred while retrieving student balance. Please try again.",
                Error = OAuthErrorEnum.TemporarilyUnavailable.ToOAuthErrorString()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError("Unexpected error occurred. ExceptionType: {ExceptionType}, StackTrace: {StackTrace}", ex.GetType().Name, ex.StackTrace);
            return new GetStudentBalanceResponseDto
            {
                Success = false,
                Message = "An Unexpected error occurred.",
                Error = OAuthErrorEnum.ServerError.ToOAuthErrorString()
            };
        }
    }
    public async Task<PaginatedResultDto<GetStudentsDuesResponseDto>> GetAllStudentDuesAsync(GetStudentsDuesRequestDto dto)
    {
        try
        {
            var query = _context.StudentDues
                .Include(sd => sd.Student)
                .OrderByDescending(sd => sd.CreatedAt);

            var totalCount = await query.CountAsync();

            var dues = await query
                .Skip((dto.Page - 1) * dto.PageSize)
                .Take(dto.PageSize)
                .Select(sd => new GetStudentsDuesResponseDto
                {
                    AdmissionNumber = sd.Student.AdmissionNumber,
                    FirstName = sd.Student.FirstName,
                    LastName = sd.Student.LastName,
                    DuesType = sd.DuesType,
                    DuesAmount = sd.DuesAmount,
                    EffectiveDate = sd.EffectiveDate,
                    CreatedAt = sd.CreatedAt
                })
                .ToListAsync();

            return new PaginatedResultDto<GetStudentsDuesResponseDto>
            {
                TotalCount = totalCount,
                Page = dto.Page,
                PageSize = dto.PageSize,
                Items = dues
            };
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError("Error retrieving Dues. ExceptionType{ExceptionType}, StackTrace {StackTrace}", ex.GetType().Name, ex.StackTrace);
            return new PaginatedResultDto<GetStudentsDuesResponseDto>
            {
                Error = OAuthErrorEnum.TemporarilyUnavailable,
                Message = "A database error occurred while retrieving student dues. Please try again."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError("An unexpected error occurred: ExceptionType: {ExceptionType}, StackTrace: {StackTrace}", ex.GetType().Name, ex.StackTrace);
            return new PaginatedResultDto<GetStudentsDuesResponseDto>
            {
                Error = OAuthErrorEnum.ServerError,
                Message = $"An unexpected error occurred while retrieving student dues: {ex.Message}. Please try again."
            };
        }
    }
}