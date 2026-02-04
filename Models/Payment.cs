namespace StudentPayments_API.Models;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

//Inform EF Core how c# maps to database
//Maps Payment entity to payments table in the database
[Table("payments")]
public class Payment
{
    [Column("payment_id")]
    public int PaymentId {get; set;}

    [Column("reference_number")]
    public string ReferenceNumber {get;set;}

    [Column("payment_datetime")]
    public DateTime PaymentDateTime {get; set;}

    [Column("payment_type")]
    public PaymentTypeEnum PaymentType {get; set;}

    [Column("payment_channel")]
    public PaymentChannelEnum PaymentChannel {get; set;}

    [Column("student_id")]
    public int StudentId {get; set;}

    [Column("admission_number")]
    public string AdmissionNumber {get; set;}

    [Column("amount")]
    public decimal Amount {get; set;}

    [Column("created_at")]
    public DateTime CreatedAt {get; set;}

    [Column("updated_at")]
    public DateTime UpdatedAt {get; set;}
    [Column("bank_client_id")]
    public int? BankClientId {get;set;}
    public BankClient BankClient {get;set;}
}