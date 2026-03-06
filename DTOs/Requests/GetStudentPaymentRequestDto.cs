using System.ComponentModel.DataAnnotations;

namespace StudentPayments_API.DTOs.Requests;

public class GetStudentPaymentsRequestDto
{
    [Required]
    public string AdmissionNumber {get;set;}
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
    public int Page {get;set;} = 1;
    [Range(1,20, ErrorMessage ="PageSize must be between 1 and 20.")]
    public int PageSize {get;set;}
}