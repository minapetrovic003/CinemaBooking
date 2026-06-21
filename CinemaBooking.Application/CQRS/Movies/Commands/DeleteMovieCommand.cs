using MediatR;

namespace CinemaBooking.Application.CQRS.Movies.Commands;

public record DeleteMovieCommand(long Id) : IRequest<bool>;