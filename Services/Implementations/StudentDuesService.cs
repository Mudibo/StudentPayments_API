using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.DTOs.Responses;
using StudentPayments_API.Models;
using StudentPayments_API.Services.Interfaces;
using StudentPayments_API.Data;

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
    public async Task<AddStudentDuesResponseDto<StudentDues>> AddDuesAsync(AddStudentDuesDto dto)
    {
        try
        {
            var response = new AddStudentDuesResponseDto<StudentDues>();
        var student = await _context.Students.FirstOrDefaultAsync(s => s.AdmissionNumber == dto.AdmissionNumber.Trim());
        if(student == null)
        {
            response.Success = false;
            response.Message = "Student not found for the given admission number.";
            response.Data = null;
            return response;
        }
        var dues = new StudentDues
        {
            StudentId = student.StudentId,
            DuesAmount = dto.DuesAmount,
            DuesType = dto.DuesType,
            EffectiveDate = dto.EffectiveDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow            
        };
        _context.StudentDues.Add(dues);
        await _context.SaveChangesAsync();

        response.Success = true;
        response.Message = "Student dues added successfully.";
        response.Data = dues;
        return response;
        }catch(DbUpdateException dbEx)
        {
            var response = new AddStudentDuesResponseDto<StudentDues>();
            response.Success = false;
            response.Message = "A database error occurred while adding student dues.";
            response.Data = null;
            _logger.LogError(dbEx, "Database error occurred while adding dues for AdmissionNumber: {AdmissionNumber}", dto.AdmissionNumber);
            return response;
        }catch(Exception ex)
        {
            var response = new AddStudentDuesResponseDto<StudentDues>();
            response.Success = false;
            response.Message = "An unexpected error occurred while adding student dues.";
            response.Data = null;
            _logger.LogError(ex, "An unexpected error occurred while adding dues for AdmissionNumber: {AdmissionNumber}", dto.AdmissionNumber);
            return response;
        }
    }
}