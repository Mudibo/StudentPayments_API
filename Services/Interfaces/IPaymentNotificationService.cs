using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.DTOs.Responses;

namespace StudentPayments_API.Services.Interfaces;

public interface IPaymentNotificationService
{
    Task<PaymentNotificationResponseDto> ProcessNotificationAsync(PaymentNotificationRequestDto dto, string idempotencyKey, string clientId);
}