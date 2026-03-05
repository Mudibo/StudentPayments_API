namespace StudentPayments_API.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using StudentPayments_API.Models.Enums;
using System.ComponentModel.DataAnnotations;


[Table("students")]
public class Student
{
    public int StudentId {get; set;}
    [Required, MaxLength(20)]
    public string AdmissionNumber {get; set;}
    [Required, MaxLength(50)]
    public string FirstName {get; set;}
    [Required, MaxLength(50)]
    public string LastName {get; set;}
    [Required, MaxLength(100), EmailAddress]
    public string Email {get; set;}
    [Required, MaxLength(20), Phone]
    public string MobileNumber {get; set;}
    [Required]
    public ProgramEnum Program {get; set;}
    [Required]
    public EnrollmentStatusEnum EnrollmentStatus { get; set; }
    public DateTime CreatedAt {get; set;}
    public DateTime UpdatedAt {get; set;}
    [JsonIgnore, MaxLength(255)]
    public string PasswordHash {get; set;}
    [Required, MaxLength(20)]
    public string Role { get; set; }
    public ICollection<PaymentTransaction> PaymentTransactions { get; set; }
    public ICollection<StudentDues> StudentDues {get; set;}
}