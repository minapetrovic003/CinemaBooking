using CinemaBooking.API.CQRS.Movies.Commands;
using CinemaBooking.Domain.Repositories;
using MediatR;

namespace CinemaBooking.API.CQRS.Movies.Handlers;

public class DeleteMovieHandler : IRequestHandler<DeleteMovieCommand, bool>
{
    private readonly IUnitOfWork _uow;

    public DeleteMovieHandler(IUnitOfWork uow) => _uow = uow;

    public Task<bool> Handle(DeleteMovieCommand request, CancellationToken cancellationToken)
    {
        var movie = _uow.Movies.GetById(request.Id);
        if (movie is null)
            return Task.FromResult(false);

        _uow.Movies.Remove(movie);
        _uow.SaveChanges();
        return Task.FromResult(true);
    }
}