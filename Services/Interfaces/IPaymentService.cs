using StudentPayments_API.DTOs.Requests;
using System.Threading.Tasks;
using StudentPayments_API.Models;

namespace StudentPayments_API.Services.Interfaces;

public interface IPaymentService
{
    Task<(bool success, string message)> RegisterPaymentAsync(PaymentDto dto);
    Task<List<Payment>> GetPaymentForStudentAsync(int studentId);
}