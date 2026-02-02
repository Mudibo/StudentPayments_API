using System;

namespace StudentPayments_API.DTOs.Requests;

public class AddStudentDuesDto
{
    public string AdmissionNumber {get;set;}
    public decimal DuesAmount {get;set;}
    public DateTime EffectiveDate {get;set;}
    public string DuesType {get;set;}
}