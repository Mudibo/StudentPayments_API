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
    public StudentDuesService(StudentPaymentsDbContext context)
    {
        _context = context;
    }
    public async Task<AddStudentDuesResponseDto<StudentDues>> AddDuesAsync(AddStudentDuesDto dto)
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
    }
}