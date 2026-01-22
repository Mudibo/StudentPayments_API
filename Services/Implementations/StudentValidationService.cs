using Microsoft.EntityFrameworkCore;
using StudentPayments_API.Models;
using StudentPayments_API.Data;
using StudentPayments_API.Services.Interfaces;
using System.Reflection;
using System.Runtime.Serialization;

namespace StudentPayments_API.Services.Implementations;
public class StudentValidationService : IStudentValidationService
{
    private readonly StudentPaymentsDbContext _context;

    //Inject the DbContext for database access
    //Constructor receives the DbContext and stores it
    public StudentValidationService(StudentPaymentsDbContext context)
    {
        _context = context;
    }
    private static bool TryParseEnumMember<TEnum>(string value, out TEnum result) where TEnum : struct
        {
        foreach (var field in typeof(TEnum).GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field, typeof(EnumMemberAttribute)) as EnumMemberAttribute;
                    if (attribute != null)
                    {
                        if (string.Equals(attribute.Value, value, StringComparison.OrdinalIgnoreCase))
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
    public async Task<(bool isValid, Student? student, string message)> ValidateStudentAsync(string admissionNumber, string program, string mobileNumber)
    {
        //Validation check to ensure that all fields are provided
        if(string.IsNullOrEmpty(admissionNumber) || string.IsNullOrEmpty(program) || string.IsNullOrEmpty(mobileNumber))
            return (false, null, "Required information not provided.");
        
        
        if (!TryParseEnumMember<ProgramEnum>(program, out var programEnum))
            return (false, null, "Invalid program value.");

        // Debug print to help diagnose matching issues
        Console.WriteLine($"admissionNumber: '{admissionNumber}', program: '{program}', mobileNumber: '{mobileNumber}', programEnum: '{programEnum}'");
        
        //Query the database for a student matching all three fields and is active
        var student = await _context.Students
        .FirstOrDefaultAsync(s =>
            s.AdmissionNumber == admissionNumber &&
            s.Program == programEnum &&
            s.MobileNumber == mobileNumber &&
            s.EnrollmentStatus == "Active");
        //If student not found, returns false and a message
        if (student == null)
            return (false, null, "Student not found or not active");

        //If the student is found, returns true, the student object, and a success message
        return (true, student, "Student is valid");
    }
}