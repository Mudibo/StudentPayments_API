using System.Collections.Generic;
using StudentPayments_API.Models.Enums;

namespace StudentPayments_API.DTOs.Responses;

public class PaginatedResultDto<T>
{
    public int TotalCount {get;set;}
    public OAuthErrorEnum? Error {get;set;}
    public string? Message {get;set;}
    public int Page {get;set;}
    public int PageSize {get;set;}
    public List<T> Items {get;set;}
}