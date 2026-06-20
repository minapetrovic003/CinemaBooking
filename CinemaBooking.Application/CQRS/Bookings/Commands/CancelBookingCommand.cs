using MediatR;

namespace CinemaBooking.Application.CQRS.Bookings.Commands;

public record CancelBookingCommand(long Id) 
    : IRequest<(bool Success, string? ErrorMessage)>;