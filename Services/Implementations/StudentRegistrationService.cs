using Microsoft.EntityFrameworkCore;
using StudentPayments_API.Models;
using StudentPayments_API.Data;
using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.Services.Interfaces;
using BCrypt.Net;

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

    //Method to convert string to enum member based on PgName attribute
    private static bool TryParseEnumMember<TEnum>(string value, out TEnum result) where TEnum : struct
        {
        foreach (var field in typeof(TEnum).GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field, typeof(NpgsqlTypes.PgNameAttribute)) as NpgsqlTypes.PgNameAttribute;
                    if (attribute != null)
                    {
                        if (string.Equals(attribute.PgName, value, StringComparison.OrdinalIgnoreCase))
                            {
                                result = (TEnum)field.GetValue(null);
                                    return true;
                            }                   
                    }
                    else
                    {
                        if (string.Equals(field.Name, value, StringComparison.OrdinalIgnoreCase))
                            {
                                result = (TEnum)field.GetValue(null);
                                    return true;
                            }
                    }
                }
                    result = default;
                    return false;
        }
    public async Task<(bool success, string message, Student? student)> RegisterStudentAsync(StudentRegistrationDto dto){
        try {
            //Check for duplicate admission number
            if(await _context.Students.AnyAsync(s => s.AdmissionNumber == dto.AdmissionNumber.Trim()))
            {
                _logger.LogWarning("Attempted to register duplicate student with admission number: {admissionNumber}", dto.AdmissionNumber.Trim());
                return (false, "A student with the same admission number already exists", null);
            }
            //Validate and parse Program enum
            if (!TryParseEnumMember<ProgramEnum>(dto.Program, out var programEnum))
            {
                _logger.LogWarning("Invalid program value: {program} for AdmissionNumber: {AdmissionNumber}", dto.Program, dto.AdmissionNumber.Trim());
                return (false, "Invalid program value.", null);
            }
            //Validate and parse EnrollmentStatus enum
            if(!TryParseEnumMember<EnrollmentStatusEnum>(dto.EnrollmentStatus, out var enrollmentStatusEnum))
            {
                _logger.LogWarning("Invalid enrollment status value: {enrollmentStatus} for AdmissionNumber: {AdmissionNumber}", dto.EnrollmentStatus, dto.AdmissionNumber.Trim());
                return (false, "Invalid enrollment status value.", null);
            }
            var trimmedPassword = dto.Password.Trim();
            //Hash the password before storing in the database
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(trimmedPassword);
           
            var student = new Student 
            {
                AdmissionNumber = dto.AdmissionNumber.Trim(),
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Email = dto.Email.Trim(),
                MobileNumber = dto.MobileNumber.Trim(),
                Program = programEnum,
                EnrollmentStatus = enrollmentStatusEnum,
                ExternalID = dto.ExternalID,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PasswordHash = passwordHash,
                Role = "Student"
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Student registered successfully with AdmissionNumber: {AdmissionNumber}", dto.AdmissionNumber.Trim());
            return (true, "Student Registered Successfully", student);

        } catch (DbUpdateException dbEx) {
            _logger.LogError(dbEx, "A database error occurred while registering the student");
            if(dbEx.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true || 
                dbEx.InnerException?.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true)
            {
                return (false, "A student with the same admission number already exists.", null);
            }
            return (false, "A database error occurred while registering the student.",null);
        } catch (Exception ex){
            _logger.LogError(ex, "An unexpected error occurred while registering the student");
            return (false, "An unexpected error occurred while registering the student.", null);
        }
    }
}