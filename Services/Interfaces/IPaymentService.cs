using StudentPayments_API.DTOs.Requests;
using System.Threading.Tasks;

namespace StudentPayments_API.Services.Interfaces;

public interface IPaymentService
{
    Task<(bool success, string message)> RegisterPaymentAsync(PaymentDto dto);
}