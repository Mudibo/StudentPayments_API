namespace StudentPayments_API.DTOs.Requests;

public class  CreateBankClientDto
{
    public string BankName {get;set;} 
    public string ClientId {get;set;}
    public string ClientSecret {get;set;}
}