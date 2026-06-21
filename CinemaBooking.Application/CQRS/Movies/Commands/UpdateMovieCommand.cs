using MediatR;

namespace CinemaBooking.Application.CQRS.Movies.Commands;

public record UpdateMovieCommand(
    long Id,
    string Title,
    string Description,
    string Genre,
    int DurationMinutes,
    decimal Rating
) : IRequest<bool>;