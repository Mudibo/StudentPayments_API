namespace StudentPayments_API.DTOs.Responses;

public class AddStudentDuesResponseDto<T>
{
    public bool Success {get;set;}
    public string Message {get;set;}
    public T Data {get;set;}
}