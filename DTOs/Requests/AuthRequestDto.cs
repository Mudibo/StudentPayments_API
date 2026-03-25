using System.ComponentModel.DataAnnotations;

namespace StudentPayments_API.DTOs.Requests;
public class AuthRequestDto
{
    [Required]
    public string AdmissionNumber { get; set; }
    [Required]
    public string Password { get; set; }
}