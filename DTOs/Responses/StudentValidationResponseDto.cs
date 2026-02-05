using StudentPayments_API.Models;
namespace StudentPayments_API.DTOs.Responses;
public class StudentValidationResponseDto
{
    public StudentValidationStatus Status {get;set;}
    public string Message {get;set;}
    public string StudentName {get;set;}
    public string Program {get;set;}
}