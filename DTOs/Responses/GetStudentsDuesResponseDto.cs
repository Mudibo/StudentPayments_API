using System;
using StudentPayments_API.Models.Enums;
namespace StudentPayments_API.DTOs.Responses;

public class GetStudentsDuesResponseDto
{
    public string AdmissionNumber {get;set;}
    public string FirstName {get;set;}
    public string LastName {get;set;}
    public DuesTypeEnum DuesType {get;set;}
    public decimal DuesAmount {get;set;}
    public DateTime EffectiveDate {get;set;}
    public DateTime CreatedAt {get;set;}
}