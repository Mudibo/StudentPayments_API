using System.ComponentModel.DataAnnotations;

namespace StudentPayments_API.DTOs.Requests;

public class GetStudentsDuesRequestDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 1.")]
    public int Page { get; set; } = 1;

    [Range(1, 50, ErrorMessage = "Page size must be between 1 and 50.")]
    public int PageSize { get; set; } = 20;
}