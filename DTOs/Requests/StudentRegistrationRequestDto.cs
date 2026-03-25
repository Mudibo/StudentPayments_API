
namespace StudentPayments_API.DTOs.Requests;

using System.ComponentModel.DataAnnotations;

//DTO (Data Transfer Object) for student registration requests
// Defines the data structure for registration requests
public class StudentRegistrationDto
{
    [Required]
    public string AdmissionNumber { get; set; }
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }

    [EmailAddress]
    [Required]
    public string? Email { get; set; }
    [Required]
    public string MobileNumber { get; set; }
    [Required]
    public string Program { get; set; }
    [Required]
    public string EnrollmentStatus { get; set; }
    [Required]
    public string Password { get; set; }
}