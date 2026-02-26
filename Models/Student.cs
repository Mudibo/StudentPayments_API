namespace StudentPayments_API.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using StudentPayments_API.Models.Enums;

//Class that represents a student in the system and maps directly to the students table in the database
//Each property represents a column in the database table
//It is a data model that defines what information is stored about each student
[Table("students")]
public class Student
{
    [Column("student_id")]
    public int StudentId {get; set;}

    [Column("admission_number")]
    public string AdmissionNumber {get; set;}
    
    [Column("first_name")]
    public string FirstName {get; set;}

    [Column("last_name")]
    public string LastName {get; set;}

    [Column("email")]
    public string? Email {get; set;}

    [Column("mobile_number")]
    public string MobileNumber {get; set;}

    [Column("program")]
    public ProgramEnum Program {get; set;}

    [Column("enrollment_status")]
    public EnrollmentStatusEnum EnrollmentStatus { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt {get; set;}

    [Column("updated_at")]
    public DateTime UpdatedAt {get; set;}
    
    [JsonIgnore]
    [Column("password_hash")]
    public string PasswordHash {get; set;}

    [Column("role")]
    public string Role { get; set; }

    public ICollection<StudentDues> StudentDues {get; set;}
}