using Microsoft.EntityFrameworkCore;
using StudentPayments_API.Models;
using StudentPayments_API.Data;
using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.Services.Interfaces;

namespace StudentPayments_API.Services.Implementations;

public class StudentRegistrationService : IStudentRegistrationService
{
    private readonly StudentPaymentsDbContext _context;

    public StudentRegistrationService(StudentPaymentsDbContext context)
    {
        _context = context;
    }
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
    public async Task<(bool success, string message, Student? student)> RegisterStudentAsync(StudentRegistrationDto dto)
    {
        if (await _context.Students.AnyAsync(s => s.AdmissionNumber == dto.AdmissionNumber))
            return (false, "A student with this admission number already exists.", null);

        if(!TryParseEnumMember<ProgramEnum>(dto.Program, out var programEnum))
            return (false, "Invalid Program value.", null);

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

        return (true, "Student registered successfully.", student);
    }
}