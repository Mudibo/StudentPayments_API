namespace StudentPayments_API.DTOs.Responses;
public class AddBankClientResponseDto
{
    public bool Success {get;set;}
    public string Message {get;set;}
    public string BankName {get;set;}
    public bool IsActive {get;set;}
}