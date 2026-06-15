using CinemaBooking.API.DTOs.Movies;
using MediatR;

namespace CinemaBooking.API.CQRS.Movies.Commands;

public record CreateMovieCommand(
    string Title,
    string Description,
    string Genre,
    int DurationMinutes,
    decimal Rating
) : IRequest<MovieDto>;