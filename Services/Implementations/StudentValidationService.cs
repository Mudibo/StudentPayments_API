
using StudentPayments_API.Data;
using StudentPayments_API.Services.Interfaces;
using StudentPayments_API.DTOs.Responses;
using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace StudentPayments_API.Services.Implementations;
public class StudentValidationService : IStudentValidationService
{
    private readonly StudentPaymentsDbContext _context;
    private readonly ILogger<StudentValidationService> _logger;

    //Inject the DbContext for database access
    //Constructor receives the DbContext and stores it
    public StudentValidationService(StudentPaymentsDbContext context, ILogger<StudentValidationService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<StudentValidationResponseDto> ValidateStudentAsync(StudentValidationRequestDto dto)
    {
        try {
            if (string.IsNullOrWhiteSpace(dto.AdmissionNumber))
            {
                _logger.LogWarning("Admission number is required but was not provided.");
                return new StudentValidationResponseDto {
                    Status = StudentValidationStatus.Error,
                    Message = "Admission number is required",
                    StudentName = null,
                    Program = null
                };
            }
            var student = await _context.Students
                .Where(s => s.AdmissionNumber == dto.AdmissionNumber.Trim())
                .Select(s => new
                {
                    s.FirstName,
                    s.LastName,
                    s.Program,
                    s.EnrollmentStatus
                }).FirstOrDefaultAsync();
            if(student == null)
            {
                _logger.LogWarning("Student not found with AdmissionNumber: {AdmissionNumber}", dto.AdmissionNumber);
                return new StudentValidationResponseDto {
                    Status = StudentValidationStatus.NotFound,
                    Message = "Student not found",
                    StudentName = null,
                    Program = null
                };
            }
            else if (student.EnrollmentStatus != Models.EnrollmentStatusEnum.Active)
            {
                _logger.LogWarning("Student with AdmissionNumber: {AdmissionNumber} has inactive enrollment status.", dto.AdmissionNumber);
                return new StudentValidationResponseDto {
                    Status = StudentValidationStatus.Inactive,
                    Message = "Student enrollment status is not active",
                    StudentName = null,
                    Program = null
                };
            }else{
                _logger.LogInformation("Student with AdmissionNumber: {AdmissionNumber} is valid.", dto.AdmissionNumber);
                return new StudentValidationResponseDto
                {
                    Status = StudentValidationStatus.Valid,
                    Message = "Student validated successfully",
                    StudentName = $"{student.FirstName} {student.LastName}",
                    Program = student.Program.ToString()
                };
            }
        }catch(TimeoutException tex)
        {
            _logger.LogError(tex, "A timeout occurred while validating student with AdmissionNumber: {AdmissionNumber}", dto.AdmissionNumber, tex.GetType().FullName, tex.StackTrace);
            return new StudentValidationResponseDto {
                Status = StudentValidationStatus.TransientError,
                Message = "A database error occurred during student validation. Please try again.",
                StudentName = null,
                Program = null
            };
        }catch(NpgsqlException npex)
        {
            _logger.LogError(npex, "A database error occurred while validating student with AdmissionNumber: {AdmissionNumber}", dto.AdmissionNumber, npex.GetType().FullName, npex.StackTrace);
            return new StudentValidationResponseDto {
                Status = StudentValidationStatus.TransientError,
                Message = "A database error occurred during student validation. Please try again.",
                StudentName = null,
                Program = null
            };
        }catch (DbUpdateException dbEx){
            _logger.LogError(dbEx, "A database update error occurred while validating student with AdmissionNumber: {AdmissionNumber}. ExceptionType: {ExceptionType}, StackTrace: {StackTrace}", dto.AdmissionNumber, dbEx.GetType().FullName, dbEx.StackTrace);
            return new StudentValidationResponseDto
            {
                Status = StudentValidationStatus.TransientError,
                Message = "A database error occurred during student validation. Please try again.",
                StudentName = null,
                Program = null
            };
        }catch(InvalidOperationException invOpEx){
            _logger.LogError(invOpEx, "A database operation error occurred while validating student with AdmissionNumber: {AdmissionNumber}. ExceptionType: {ExceptionType}, StackTrace: {StackTrace}", dto.AdmissionNumber, invOpEx.GetType().FullName, invOpEx.StackTrace);
             return new StudentValidationResponseDto
            {
                Status = StudentValidationStatus.TransientError,
                Message = "A database error occurred during student validation. Please try again.",
                StudentName = null,
                Program = null
            };
        }catch(Exception ex){
            _logger.LogError(ex, "An error occurred while validating student with AdmissionNumber: {AdmissionNumber}", dto.AdmissionNumber, ex.GetType().FullName, ex.StackTrace);
            return new StudentValidationResponseDto {
                Status = StudentValidationStatus.Error,
                Message = "An error occurred during student validation",
                StudentName = null,
                Program = null
            };
        }
    }
}