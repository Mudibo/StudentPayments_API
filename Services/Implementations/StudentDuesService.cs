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
            if(studentData == null)
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
        }catch(DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error occurred while adding dues for AdmissionNumber: {AdmissionNumber}, ExceptionType: {ExceptionType}, StackTrace: {StackTrace}", dto.AdmissionNumber, dbEx.GetType().Name, dbEx.StackTrace);
            return new AddStudentDuesResponseDto
            {
                Success = false,
                Error = OAuthErrorEnum.TemporarilyUnavailable.ToOAuthErrorString(),
                Message = "A database error occurred while adding student dues. Please try again."
            };       
        }catch(Exception ex)
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
    public async Task<decimal> GetStudentBalanceAsync(GetStudentBalanceRequestDto dto)
    {
        var totalDues = await _context.StudentDues
            .Where(d => d.StudentId == dto.StudentId)
            .SumAsync(d => d.DuesAmount);

        var totalPaid = await _context.Payments
            .Where(p => p.StudentId == dto.StudentId)
            .SumAsync(p => p.Amount);

        return totalDues - totalPaid;
    }
}