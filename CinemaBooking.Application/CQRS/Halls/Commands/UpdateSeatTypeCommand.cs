using MediatR;

namespace CinemaBooking.Application.CQRS.Halls.Commands;

public record UpdateSeatTypeCommand(
    long HallId,
    long SeatId,
    string SeatType
) : IRequest<(bool Success, string? ErrorMessage, int StatusCode)>;