namespace StudentPayments_API.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using StudentPayments_API.Models.Enums;


[Table("students")]
public class Student
{
    public int StudentId {get; set;}
    public string AdmissionNumber {get; set;}
    public string FirstName {get; set;}
    public string LastName {get; set;}
    public string Email {get; set;}
    public string MobileNumber {get; set;}
    public ProgramEnum Program {get; set;}
    public EnrollmentStatusEnum EnrollmentStatus { get; set; }
    public DateTime CreatedAt {get; set;}
    public DateTime UpdatedAt {get; set;}
    [JsonIgnore]
    public string PasswordHash {get; set;}
    public string Role { get; set; }
    public ICollection<PaymentTransaction> PaymentTransactions { get; set; }
    public ICollection<StudentDues> StudentDues {get; set;}
}