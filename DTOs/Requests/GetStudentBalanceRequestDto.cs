namespace StudentPayments_API.DTOs.Requests;
using System.ComponentModel.DataAnnotations;

public class GetStudentBalanceRequestDto
{
    [Required]
    public string AdmissionNumber {get; set;}
}