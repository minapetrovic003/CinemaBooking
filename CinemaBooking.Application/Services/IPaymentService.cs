using CinemaBooking.Application.DTOs.Payments;

namespace CinemaBooking.Application.Services;

public interface IPaymentService
{
    IEnumerable<PaymentDto> GetAll();
    Task<PaymentDto?> GetByIdAsync(long id);
    Task<(PaymentDto? dto, string? errorMessage, int statusCode)> CreateAsync(CreatePaymentRequest request);
    Task<(bool success, string? errorMessage)> RefundAsync(long id);
}