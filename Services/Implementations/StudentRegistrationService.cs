using Microsoft.EntityFrameworkCore;
using StudentPayments_API.Models;
using StudentPayments_API.Data;
using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.Services.Interfaces;

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
            if(await _context.Students.AnyAsync(s => s.AdmissionNumber == dto.AdmissionNumber))
                return (false, "A student with the same admission number already exists", null);
            
            //Validate and parse Program enum
            if (!TryParseEnumMember<ProgramEnum>(dto.Program, out var programEnum))
                return (false, "Invalid program value.", null);

            //Validate and parse EnrollmentStatus enum
            if(!TryParseEnumMember<EnrollmentStatusEnum>(dto.EnrollmentStatus, out var enrollmentStatusEnum))
                return (false, "Invalid enrollment status value.", null);

            var student = new Student 
            {
                AdmissionNumber = dto.AdmissionNumber,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                MobileNumber = dto.MobileNumber,
                Program = programEnum,
                EnrollmentStatus = enrollmentStatusEnum,
                ExternalID = dto.ExternalID,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return (true, "Student Registered Successfully", student);

        } catch (DbUpdateException dbEx) {
            _logger.LogError(dbEx, "A database error occurred while registering the student");
            return (false, "A database error occurred while registering the student.",null);
        } catch (Exception ex){
            _logger.LogError(ex, "An unexpected error occurred while registering the student");
            return (false, "An unexpected error occurred while registering the student.", null);
        }
    }
}