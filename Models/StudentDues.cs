using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using StudentPayments_API.Models.Enums;
namespace StudentPayments_API.Models;

public class StudentDues
{
    [Key]
    public int DueId { get; set; }
    [Required]
    public int StudentId { get; set; }
    [Required]
    public decimal DuesAmount { get; set; }
    [Required]
    public DateTime EffectiveDate { get; set; }
    [Required]
    public DuesTypeEnum DuesType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Student Student { get; set; }
}