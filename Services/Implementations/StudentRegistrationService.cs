using Microsoft.EntityFrameworkCore;
using StudentPayments_API.Models;
using StudentPayments_API.Data;
using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.Services.Interfaces;
using StudentPayments_API.DTOs.Responses;
using StudentPayments_API.Models.Enums;
using StudentPayments_API.Utils;
using BCrypt.Net;
using StudentPayments_API.Models.Enums;
namespace StudentPayments_API.Services.Implementations;

//Service that handles business logic for student registration: Enum conversion, duplicate checking, database persistence

public class StudentRegistrationService : IStudentRegistrationService
{
    private readonly StudentPaymentsDbContext _context;
    private readonly ILogger<StudentRegistrationService> _logger;

    //Constructor that receives DbContext via dependency injection
    public StudentRegistrationService(StudentPaymentsDbContext context, ILogger<StudentRegistrationService> logger)
    {
        _context = context;
        _logger = logger;
    }
    public async Task<StudentRegistrationResponseDto> RegisterStudentAsync(StudentRegistrationDto dto)
    {
        //Trim all string fields
        dto.AdmissionNumber = dto.AdmissionNumber?.Trim();
        dto.FirstName = dto.FirstName?.Trim();
        dto.LastName = dto.LastName?.Trim();
        dto.Email = dto.Email?.Trim();
        dto.MobileNumber = dto.MobileNumber?.Trim();
        dto.Program = dto.Program?.Trim();
        dto.EnrollmentStatus = dto.EnrollmentStatus?.Trim();
        dto.Password = dto.Password?.Trim();
        try
        {
            //Check for duplicate admission number
            if(await _context.Students.AnyAsync(s => s.AdmissionNumber == dto.AdmissionNumber.Trim())){
                _logger.LogWarning("Attempting to register a student with a duplicate admission number: {AdmissionNumber}", dto.AdmissionNumber);
                return new StudentRegistrationResponseDto
                {
                    Success = false,
                    Message = "Admission Number already exists.",
                    Error = OAuthErrorEnum.Conflict.ToOAuthErrorString()
                };
            }
            if(!EnumParse.TryParseEnumMember<ProgramEnum>(dto.Program, out var programEnum)){
                _logger.LogWarning("Invalid Program Value: {program}", dto.Program);
                return new StudentRegistrationResponseDto {
                    Success = false,
                    Message = "Invalid Program value.",
                    Error = OAuthErrorEnum.InvalidRequest.ToOAuthErrorString()
                };
            }
            if(!EnumParse.TryParseEnumMember<EnrollmentStatusEnum>(dto.EnrollmentStatus, out var enrollmentStatusEnum)){
                _logger.LogWarning("Invalid Enrollment Status Value: {enrollmentStatus}", dto.EnrollmentStatus);
                return new StudentRegistrationResponseDto {
                    Success = false,
                    Message = "Invalid Enrollment Status value.",
                    Error = OAuthErrorEnum.InvalidRequest.ToOAuthErrorString()
                };
            }
            var trimmedPassword = dto.Password.Trim();
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(trimmedPassword);

            var student = new Student 
            {
                AdmissionNumber = dto.AdmissionNumber.Trim(),
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Email = dto.Email,
                MobileNumber = dto.MobileNumber.Trim(),
                Program = programEnum,
                EnrollmentStatus = enrollmentStatusEnum,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Role = "Student"
            };
            try{
                _context.Students.Add(student);
                await _context.SaveChangesAsync();
            }catch(DbUpdateException dbEx){
                _logger.LogError("A database Error occurred while tring to register Admission Number: {Admission Number}, ExceptionType: {ExceptionType}, StackTrace: {StackTrace}", student.AdmissionNumber, dbEx.GetType().Name, dbEx.StackTrace);
                return new StudentRegistrationResponseDto
                {
                    Success = false,
                    Message = "A database error occurred while trying to register the student.",
                    Error = OAuthErrorEnum.TemporarilyUnavailable.ToOAuthErrorString()
                };
            }catch(Exception ex)
            {
                _logger.LogError("An unexpected error occurred while trying to register Admission Number: {Admission Number}, ExceptionType: {ExceptionType}, StackTrace: {StackTrace}", student.AdmissionNumber, ex.GetType().Name, ex.StackTrace);
                return new StudentRegistrationResponseDto
                {
                    Success = false,
                    Message = "An unexpected error occurred while trying to register the student.",
                    Error = OAuthErrorEnum.ServerError.ToOAuthErrorString()
                };
            }
            return new StudentRegistrationResponseDto
            {
                Success = true,
                Message = "Student registered successfully.",
                Error = OAuthErrorEnum.None.ToOAuthErrorString()
            };
        }catch(DbUpdateException dbEx)
        {
            _logger.LogError("A database error occurred while trying to register a student. ExceptionType: {ExceptionType}, StackTrace: {StackTrace}", dbEx.GetType().Name, dbEx.StackTrace);
            return new StudentRegistrationResponseDto
            {
                Success = false,
                Message = "A database error occurred while trying to register the student.",
                Error = OAuthErrorEnum.TemporarilyUnavailable.ToOAuthErrorString()
            };
        }
        catch(Exception ex)
        {
            _logger.LogError("An unexpected error occurred while trying to register a student. ExceptionType: {ExceptionType}, StackTrace: {StackTrace}", ex.GetType().Name, ex.StackTrace);
            return new StudentRegistrationResponseDto
            {
                Success = false,
                Message = "An unexpected error occurred while trying to register the student.",
                Error = OAuthErrorEnum.ServerError.ToOAuthErrorString()
            };
        }
    }
}