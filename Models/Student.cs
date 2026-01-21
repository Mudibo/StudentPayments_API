namespace StudentPayments_API.Models;
using System.ComponentModel.DataAnnotations.Schema;

//Class that represents a student in the system and maps directly to the students table in the database
//Each property represents a column in the database table
//It is a data mpdel that defines what information is stored about each student
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
    public string Email {get; set;}

    [Column("mobile_number")]
    public string MobileNumber {get; set;}

    [Column("program")]
    public string Program {get; set;}

    [Column("enrollment_status")]
    public string EnrollmentStatus {get; set;}

    [Column("external_student_id")]
    public int ExternalID {get;set;}

    [Column("created_at")]
    public DateTime CreatedAt {get; set;}

    [Column("updated_at")]
    public DateTime UpdatedAt {get; set;}
}