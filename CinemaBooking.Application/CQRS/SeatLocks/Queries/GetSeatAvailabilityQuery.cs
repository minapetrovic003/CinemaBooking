using CinemaBooking.Domain.DTOs.SeatLocks;
using MediatR;

namespace CinemaBooking.Application.CQRS.SeatLocks.Queries;

public record GetSeatAvailabilityQuery(
    long ShowtimeId,
    string? CurrentUserId
) : IRequest<(IEnumerable<SeatAvailabilityDto>? Result, string? ErrorMessage, int StatusCode)>;