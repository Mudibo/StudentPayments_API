using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.Models;
using StudentPayments_API.Data;
using Microsoft.EntityFrameworkCore;
using StudentPayments_API.Services.Interfaces;
using StudentPayments_API.Models.Enums;
namespace StudentPayments_API.Services.Implementations;

public class PaymentService : IPaymentService
{
    private readonly StudentPaymentsDbContext _context;
    private readonly ILogger<PaymentService> _logger;
    public PaymentService(StudentPaymentsDbContext context, ILogger<PaymentService> logger)
    {
        _context = context;
        _logger = logger;
    }
    public async Task<(bool success, string message)> RegisterPaymentAsync(PaymentDto dto)
    {
        try
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.AdmissionNumber == dto.AdmissionNumber.Trim());
            if(student == null)
            {
                _logger.LogWarning("Attempted to register payment for unregistered student with AdmissionNumber: {AdmissionNumber}", dto.AdmissionNumber.Trim());
                return (false, "Student not registered");
            }
            if (!TryParseEnumMember<PaymentTypeEnum>(dto.PaymentType, out var paymentType))
            {
                _logger.LogWarning("Invalid Payment Type: {paymentType} for AdmissionNumber: {AdmissionNumber}", dto.PaymentType, dto.AdmissionNumber.Trim());
                return (false, "Invalid payment type.");
            }
            if (!TryParseEnumMember<PaymentChannelEnum>(dto.PaymentChannel, out var paymentChannel)){
                _logger.LogWarning("Invalid Payment Channel: {paymentChannel} for AdmissionNumber: {AdmissionNumber}", dto.PaymentChannel, dto.AdmissionNumber.Trim());
                return (false, "Invalid payment channel.");
            }
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

            _logger.LogInformation("Payment registered successfully for AdmissionNumber: {AdmissionNumber}, ReferenceNumber: {ReferenceNumber}", dto.AdmissionNumber, dto.ReferenceNumber.Trim());
            return (true, "Payment registered successfully.");
        }catch(DbUpdateException dbEx){
            _logger.LogError(dbEx, "Database error occurred while registering payment for AdmissionNumber: {AdmissionNumber}", dto.AdmissionNumber);
            return (false, "A database error occurred while registering the payment");
        }catch(Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while registering payment for AdmissionNumber: {AdmissionNumber}", dto.AdmissionNumber);
            return (false, "An unexpected error occurred while registering the payment.");
        }  
    }
    public async Task<List<Payment>> GetPaymentsForStudentAsync(int studentId)
    {
        return await _context.Payments.Where(p => p.StudentId == studentId).ToListAsync();
    }

    public async Task<List<Payment>> GetPaymentForStudentAsync(int studentId)
    {
        return await _context.Payments
            .Where(p => p.StudentId == studentId)
            .ToListAsync();
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