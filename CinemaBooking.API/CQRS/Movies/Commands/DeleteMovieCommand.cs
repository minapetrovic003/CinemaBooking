using MediatR;

namespace CinemaBooking.API.CQRS.Movies.Commands;

public record DeleteMovieCommand(long Id) : IRequest<bool>;