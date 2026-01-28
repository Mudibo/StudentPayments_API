namespace StudentPayments_API.DTOs.Requests;

public class PaymentDto
{
    public string ReferenceNumber {get;set;}
    public DateTime PaymentDateTime {get;set;}
    public string PaymentType {get;set;}
    public string PaymentChannel {get; set;}
    public string AdmissionNumber {get;set;}
    public decimal Amount {get;set;}
}