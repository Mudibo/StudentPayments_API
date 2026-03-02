using System;
using System.ComponentModel.DataAnnotations;

namespace StudentPayments_API.DTOs.Requests;

public class AddStudentDuesDto
{
    [Required]
    public string AdmissionNumber {get;set;}
    [Required]
    public decimal DuesAmount {get;set;}
    [Required]
    public DateTime EffectiveDate {get;set;}
    [Required]
    public string DuesType {get;set;}
}