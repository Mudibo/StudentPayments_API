using System;
using System.ComponentModel.DataAnnotations;
using StudentPayments_API.Models.Enums;
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
    [EnumDataType(typeof(DuesTypeEnum))]
    public DuesTypeEnum DuesType {get;set;}
}