using CinemaBooking.Application.CQRS.Movies.Commands;
using CinemaBooking.Application.Repositories;
using MediatR;

namespace CinemaBooking.Application.CQRS.Movies.Handlers;

public class DeleteMovieHandler : IRequestHandler<DeleteMovieCommand, bool>
{
    private readonly IUnitOfWork _uow;

    public DeleteMovieHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<bool> Handle(DeleteMovieCommand request, CancellationToken cancellationToken)
    {
        var movie = _uow.Movies.GetById(request.Id);
        if (movie is null)
            return false;

        _uow.Movies.Remove(movie);
        await _uow.SaveChangesAsync();
        return true;
    }
}