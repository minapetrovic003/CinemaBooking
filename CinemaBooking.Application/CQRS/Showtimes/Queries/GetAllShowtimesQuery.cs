using CinemaBooking.Domain.DTOs.Showtimes;
using MediatR;

namespace CinemaBooking.Application.CQRS.Showtimes.Queries;

public record GetAllShowtimesQuery(string? MovieTitle, DateTime? FromDate)
    : IRequest<IEnumerable<ShowtimeDto>>;