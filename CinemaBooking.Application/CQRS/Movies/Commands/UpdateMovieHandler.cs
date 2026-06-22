using CinemaBooking.Application.CQRS.Movies.Commands;
using CinemaBooking.Application.Repositories;
using MediatR;

namespace CinemaBooking.Application.CQRS.Movies.Handlers;

public class UpdateMovieHandler : IRequestHandler<UpdateMovieCommand, bool>
{
    private readonly IUnitOfWork _uow;

    public UpdateMovieHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<bool> Handle(UpdateMovieCommand request, CancellationToken cancellationToken)
    {
        var movie = _uow.Movies.GetById(request.Id);
        if (movie is null)
            return false;

        movie.Title = request.Title;
        movie.Description = request.Description;
        movie.Genre = request.Genre;
        movie.DurationMinutes = request.DurationMinutes;
        movie.Rating = request.Rating;

        await _uow.SaveChangesAsync();
        return true;
    }
}