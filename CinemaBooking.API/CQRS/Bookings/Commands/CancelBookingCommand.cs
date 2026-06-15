using MediatR;

namespace CinemaBooking.API.CQRS.Bookings.Commands;

public record CancelBookingCommand(long Id) 
    : IRequest<(bool Success, string? ErrorMessage)>;