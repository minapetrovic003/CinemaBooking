using CinemaBooking.Domain.DTOs.Payments;
using MediatR;

namespace CinemaBooking.Application.CQRS.Payments.Commands;

public record CreatePaymentCommand(
    string UserEmail,
    string MovieTitle,
    string HallName,
    DateTime ShowtimeStartTime,
    string Method
) : IRequest<(PaymentDto? Dto, string? ErrorMessage, int StatusCode)>;