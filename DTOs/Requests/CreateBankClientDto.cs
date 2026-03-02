namespace StudentPayments_API.DTOs.Requests;
using System.ComponentModel.DataAnnotations;

public class  CreateBankClientDto
{
    [Required]
    public string BankName {get;set;} 
    [Required]
    public string ClientId {get;set;}
    [Required]
    public string ClientSecret {get;set;}
}