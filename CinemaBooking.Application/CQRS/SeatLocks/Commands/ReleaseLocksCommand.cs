using MediatR;

namespace CinemaBooking.Application.CQRS.SeatLocks.Commands;

public record ReleaseLocksCommand(
    string UserId,
    long ShowtimeId
) : IRequest<(bool Success, string? ErrorMessage)>;