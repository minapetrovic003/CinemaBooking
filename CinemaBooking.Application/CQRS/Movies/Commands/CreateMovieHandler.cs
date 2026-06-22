using CinemaBooking.Application.CQRS.Movies.Commands;
using CinemaBooking.Domain.DTOs.Movies;
using CinemaBooking.Domain.Models;
using CinemaBooking.Application.Repositories;
using MediatR;

namespace CinemaBooking.Application.CQRS.Movies.Handlers;

public class CreateMovieHandler : IRequestHandler<CreateMovieCommand, MovieDto>
{
    private readonly IUnitOfWork _uow;

    public CreateMovieHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<MovieDto> Handle(CreateMovieCommand request, CancellationToken cancellationToken)
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
        await _uow.SaveChangesAsync();

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

        return dto;
    }
}