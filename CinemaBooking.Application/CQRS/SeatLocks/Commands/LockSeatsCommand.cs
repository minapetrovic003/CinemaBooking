using CinemaBooking.Domain.DTOs.SeatLocks;
using MediatR;

namespace CinemaBooking.Application.CQRS.SeatLocks.Commands;

public record LockSeatsCommand(
    string UserId,
    string MovieTitle,
    string HallName,
    DateTime ShowtimeStartTime,
    List<string> Seats,
    int LockMinutes
) : IRequest<(SeatLockDto? Result, string? ErrorMessage, int StatusCode)>;