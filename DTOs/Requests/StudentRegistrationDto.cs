namespace StudentPayments_API.DTOs.Requests;

//DTO (Data Transfer Object) for student registration requests
// Defines the data structure for registration requests
public class StudentRegistrationDto
{
    public string AdmissionNumber {get; set;}
    public string FirstName {get; set;}
    public string LastName {get; set;}
    public string? Email {get; set;}
    public string MobileNumber {get; set;}
    public string Program {get; set;}
    public string EnrollmentStatus {get; set;}
    public string? ExternalID {get; set;}
    public string Password {get; set;}
}