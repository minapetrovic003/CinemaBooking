using CinemaBooking.API.DTOs.Payments;

namespace CinemaBooking.API.Services;

public interface IPaymentService
{
    Task<PaymentDto?> GetByIdAsync(long id);
    Task<(PaymentDto? dto, string? errorMessage, int statusCode)> CreateAsync(CreatePaymentRequest request);
    Task<(bool success, string? errorMessage)> RefundAsync(long id);
}