using CinemaBooking.API.CQRS.Movies.Commands;
using CinemaBooking.API.DTOs.Movies;
using CinemaBooking.Domain;
using CinemaBooking.Domain.Repositories;
using MediatR;

namespace CinemaBooking.API.CQRS.Movies.Handlers;

public class CreateMovieHandler : IRequestHandler<CreateMovieCommand, MovieDto>
{
    private readonly IUnitOfWork _uow;

    public CreateMovieHandler(IUnitOfWork uow) => _uow = uow;

    public Task<MovieDto> Handle(CreateMovieCommand request, CancellationToken cancellationToken)
    {
        var movie = new Movie
        {
            Title = request.Title,
            Description = request.Description,
            Genre = request.Genre,
            DurationMinutes = request.DurationMinutes,
            Rating = request.Rating
        };

        _uow.Movies.Add(movie);
        _uow.SaveChanges();

        var dto = new MovieDto
        {
            Id = movie.Id,
            Title = movie.Title,
            Description = movie.Description,
            Genre = movie.Genre,
            DurationMinutes = movie.DurationMinutes,
            Rating = movie.Rating,
            ShowtimeCount = 0
        };

        return Task.FromResult(dto);
    }
}