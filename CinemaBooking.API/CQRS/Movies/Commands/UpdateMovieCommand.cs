using MediatR;

namespace CinemaBooking.API.CQRS.Movies.Commands;

public record UpdateMovieCommand(
    long Id,
    string Title,
    string Description,
    string Genre,
    int DurationMinutes,
    decimal Rating
) : IRequest<bool>;