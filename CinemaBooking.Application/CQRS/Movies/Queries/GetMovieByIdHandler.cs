using CinemaBooking.Application.CQRS.Movies.Queries;
using CinemaBooking.Domain.DTOs.Movies;
using CinemaBooking.Application.Repositories;
using MediatR;

namespace CinemaBooking.Application.CQRS.Movies.Handlers;

public class GetMovieByIdHandler : IRequestHandler<GetMovieByIdQuery, MovieDto?>
{
    private readonly IUnitOfWork _uow;

    public GetMovieByIdHandler(IUnitOfWork uow) => _uow = uow;

    public Task<MovieDto?> Handle(GetMovieByIdQuery request, CancellationToken cancellationToken)
    {
        var movie = _uow.Movies.GetById(request.Id);

        if (movie is null)
            return Task.FromResult<MovieDto?>(null);

        var dto = new MovieDto
        {
            Id = movie.Id,
            Title = movie.Title,
            Description = movie.Description,
            Genre = movie.Genre,
            DurationMinutes = movie.DurationMinutes,
            Rating = movie.Rating,
            ShowtimeCount = movie.Showtimes?.Count ?? 0
        };

        return Task.FromResult<MovieDto?>(dto);
    }
}