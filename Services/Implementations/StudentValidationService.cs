
using StudentPayments_API.Data;
using StudentPayments_API.Services.Interfaces;
using StudentPayments_API.DTOs.Responses;
using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.Models;
using Microsoft.EntityFrameworkCore;

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
            var student = await _context.Students.FirstOrDefaultAsync(s => s.AdmissionNumber == dto.AdmissionNumber.Trim());
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
            else if (string.IsNullOrWhiteSpace(dto.AdmissionNumber))
            {
                _logger.LogWarning("Admission number is required but was not provided.");
                return new StudentValidationResponseDto {
                    Status = StudentValidationStatus.Error,
                    Message = "Admission number is required",
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
        }catch(Exception ex){
            _logger.LogError(ex, "An error occurred while validating student with AdmissionNumber: {AdmissionNumber}", dto.AdmissionNumber);
            return new StudentValidationResponseDto {
                Status = StudentValidationStatus.Error,
                Message = "An error occurred during student validation",
                StudentName = null,
                Program = null
            };
        }
    }
}