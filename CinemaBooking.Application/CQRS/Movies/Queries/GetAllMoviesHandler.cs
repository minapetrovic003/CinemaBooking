using CinemaBooking.Application.CQRS.Movies.Queries;
using CinemaBooking.Domain.DTOs.Common;
using CinemaBooking.Domain.DTOs.Movies;
using CinemaBooking.Domain.Repositories;
using MediatR;

namespace CinemaBooking.Application.CQRS.Movies.Handlers;

public class GetAllMoviesHandler : IRequestHandler<GetAllMoviesQuery, PagedResult<MovieDto>>
{
    private readonly IUnitOfWork _uow;

    public GetAllMoviesHandler(IUnitOfWork uow) => _uow = uow;

    public Task<PagedResult<MovieDto>> Handle(GetAllMoviesQuery request, CancellationToken cancellationToken)
    {
        var movies = _uow.Movies
            .Search(request.Title, request.Genre, request.MinRating)
            .ToList();

        var items = movies
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(m => new MovieDto
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                Genre = m.Genre,
                DurationMinutes = m.DurationMinutes,
                Rating = m.Rating,
                ShowtimeCount = m.Showtimes?.Count ?? 0
            })
            .ToList();

        var result = new PagedResult<MovieDto>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = movies.Count
        };

        return Task.FromResult(result);
    }
}