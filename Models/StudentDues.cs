using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentPayments_API.Models;

[Table("student_dues")]
public class StudentDues
{
    [Key]
    [Column("due_id")]
    public int DueId {get; set;}
    
    [Column("student_id")]
    public int StudentId {get;set;}

    [Column("dues_amount")]
    public decimal DuesAmount {get;set;}
    
    [Column("effective_date")]
    public DateTime EffectiveDate {get;set;}

    [Column("dues_type")]
    public string DuesType {get;set;}

    [Column("created_at")]
    public DateTime CreatedAt {get;set;}

    [Column("updated_at")]
    public DateTime UpdatedAt {get;set;}

    public Student Student {get;set;}
}