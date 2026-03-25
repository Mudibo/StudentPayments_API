using System.Text.Json.Serialization;
using StudentPayments_API.Models;
namespace StudentPayments_API.DTOs.Responses;
public class StudentValidationResponseDto
{
    [JsonIgnore]
    public int? StudentId { get; set; }
    public StudentValidationStatus Status { get; set; }
    public string AdmissionNumber { get; set; }
    public string Message { get; set; }
    public string StudentName { get; set; }
    public string Program { get; set; }
}