using MediatR;

namespace CinemaBooking.Application.CQRS.Bookings.Commands;

// Mrtvi parametri RequestingUserId i RequestingUserIsAdmin su uklonjeni —
// handler ih nije koristio, a controller je prosleđivao dummy vrednosti (string.Empty, false).
public record CheckInBookingCommand(long Id)
    : IRequest<(bool Success, string? ErrorMessage)>;