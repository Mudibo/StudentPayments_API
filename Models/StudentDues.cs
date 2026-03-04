using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentPayments_API.Models;

public class StudentDues
{
    [Key]
    public int DueId {get; set;}
    public int StudentId {get;set;}
    public decimal DuesAmount {get;set;}
    public DateTime EffectiveDate {get;set;}
    public string DuesType {get;set;}
    public DateTime CreatedAt {get;set;}
    public DateTime UpdatedAt {get;set;}
    public Student Student {get;set;}
}