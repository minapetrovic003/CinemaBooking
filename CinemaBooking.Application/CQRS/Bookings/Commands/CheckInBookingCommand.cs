using MediatR;

namespace CinemaBooking.Application.CQRS.Bookings.Commands;

public record CheckInBookingCommand(long Id, string RequestingUserId, bool RequestingUserIsAdmin)
    : IRequest<(bool Success, string? ErrorMessage)>;