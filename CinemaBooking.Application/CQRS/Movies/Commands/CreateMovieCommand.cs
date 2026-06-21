using CinemaBooking.Domain.DTOs.Movies;
using MediatR;

namespace CinemaBooking.Application.CQRS.Movies.Commands;

public record CreateMovieCommand(
    string Title,
    string Description,
    string Genre,
    int DurationMinutes,
    decimal Rating
) : IRequest<MovieDto>;