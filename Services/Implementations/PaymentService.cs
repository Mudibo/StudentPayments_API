using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.Models;
using StudentPayments_API.Data;
using Microsoft.EntityFrameworkCore;
using StudentPayments_API.Services.Interfaces;

namespace StudentPayments_API.Services.Implementations;

public class PaymentService : IPaymentService
{
    private readonly StudentPaymentsDbContext _context;
    public PaymentService(StudentPaymentsDbContext context)
    {
        _context = context;
    }
    public async Task<(bool success, string message)> RegisterPaymentAsync(PaymentDto dto)
    {
        var student = await _context.Students.FirstOrDefaultAsync(s => s.AdmissionNumber == dto.AdmissionNumber.Trim());
        if(student == null)
        {
            return (false, "Student not registered");
        }
        if (!TryParseEnumMember<PaymentTypeEnum>(dto.PaymentType, out var paymentType))
            return (false, "Invalid payment type.");

        if (!TryParseEnumMember<PaymentChannelEnum>(dto.PaymentChannel, out var paymentChannel))
            return (false, "Invalid payment channel.");

        var payment = new Payment
        {
            ReferenceNumber = dto.ReferenceNumber.Trim(),
            PaymentDateTime = dto.PaymentDateTime,
            PaymentType = paymentType,
            PaymentChannel = paymentChannel,
            StudentId = student.StudentId,
            AdmissionNumber = student.AdmissionNumber,
            Amount = dto.Amount,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        return (true, "Payment registered successfully.");
    }

    // Generic method to parse enums using PgName or name
    private static bool TryParseEnumMember<TEnum>(string value, out TEnum result) where TEnum : struct
    {
        foreach (var field in typeof(TEnum).GetFields())
        {
            var attribute = Attribute.GetCustomAttribute(field, typeof(NpgsqlTypes.PgNameAttribute)) as NpgsqlTypes.PgNameAttribute;
            if (attribute != null)
            {
                if (string.Equals(attribute.PgName, value, StringComparison.OrdinalIgnoreCase))
                {
                    result = (TEnum)field.GetValue(null);
                    return true;
                }
            }
            else
            {
                if (string.Equals(field.Name, value, StringComparison.OrdinalIgnoreCase))
                {
                    result = (TEnum)field.GetValue(null);
                    return true;
                }
            }
        }
        result = default;
        return false;
    }
    
}