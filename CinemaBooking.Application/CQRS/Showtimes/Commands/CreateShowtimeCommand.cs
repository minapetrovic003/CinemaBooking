using CinemaBooking.Domain.DTOs.Showtimes;
using MediatR;

namespace CinemaBooking.Application.CQRS.Showtimes.Commands;

public record CreateShowtimeCommand(
    string MovieTitle,
    string HallName,
    DateTime StartTime,
    decimal Price
) : IRequest<(ShowtimeDto? Dto, string? ErrorMessage, int StatusCode)>;