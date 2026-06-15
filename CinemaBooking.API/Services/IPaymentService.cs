using CinemaBooking.API.DTOs.Payments;

namespace CinemaBooking.API.Services;

public interface IPaymentService
{
    PaymentDto? GetById(long id);
    (PaymentDto? dto, string? errorMessage, int statusCode) Create(CreatePaymentRequest request);
    (bool success, string? errorMessage) Refund(long id);
}