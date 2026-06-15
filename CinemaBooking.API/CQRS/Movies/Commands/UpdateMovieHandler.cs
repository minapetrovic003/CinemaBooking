using CinemaBooking.API.CQRS.Movies.Commands;
using CinemaBooking.Domain.Repositories;
using MediatR;

namespace CinemaBooking.API.CQRS.Movies.Handlers;

public class UpdateMovieHandler : IRequestHandler<UpdateMovieCommand, bool>
{
    private readonly IUnitOfWork _uow;

    public UpdateMovieHandler(IUnitOfWork uow) => _uow = uow;

    public Task<bool> Handle(UpdateMovieCommand request, CancellationToken cancellationToken)
    {
        var movie = _uow.Movies.GetById(request.Id);
        if (movie is null)
            return Task.FromResult(false);

        movie.Title = request.Title;
        movie.Description = request.Description;
        movie.Genre = request.Genre;
        movie.DurationMinutes = request.DurationMinutes;
        movie.Rating = request.Rating;

        _uow.SaveChanges();
        return Task.FromResult(true);
    }
}