using MediatR;

namespace CinemaBooking.Application.CQRS.Bookings.Commands;

public record CheckInBookingCommand(long Id)
    : IRequest<(bool Success, string? ErrorMessage)>;