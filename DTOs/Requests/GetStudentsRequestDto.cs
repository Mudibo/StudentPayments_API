using System.ComponentModel.DataAnnotations;

namespace StudentPayments_API.DTOs.Requests;

public class GetStudentsRequestDto
{
    [Required]
    public int Page { get; set; }
    [Required]
    public int PageSize { get; set; }
}