using CinemaBooking.API.DTOs.Common;
using CinemaBooking.API.DTOs.Movies;
using CinemaBooking.Domain;
using CinemaBooking.Domain.Repositories;

namespace CinemaBooking.API.Services;

public class MovieService : IMovieService
{
    private readonly IUnitOfWork _uow;

    public MovieService(IUnitOfWork uow) => _uow = uow;

    public PagedResult<MovieDto> GetAll(MovieSearchRequest request)
    {
        var movies = _uow.Movies.Search(request.Title, request.Genre, request.MinRating).ToList();

        var items = movies
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(MapToDto)
            .ToList();

        return new PagedResult<MovieDto>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = movies.Count
        };
    }

    public MovieDto? GetById(long id)
    {
        var movie = _uow.Movies.GetById(id);
        return movie is null ? null : MapToDto(movie);
    }

    public MovieDto Create(CreateMovieRequest request)
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

        return MapToDto(movie);
    }

    public bool Update(long id, UpdateMovieRequest request)
    {
        var movie = _uow.Movies.GetById(id);
        if (movie is null) return false;

        movie.Title = request.Title;
        movie.Description = request.Description;
        movie.Genre = request.Genre;
        movie.DurationMinutes = request.DurationMinutes;
        movie.Rating = request.Rating;

        _uow.SaveChanges();
        return true;
    }

    public bool Delete(long id)
    {
        var movie = _uow.Movies.GetById(id);
        if (movie is null) return false;

        _uow.Movies.Remove(movie);
        _uow.SaveChanges();
        return true;
    }

    private static MovieDto MapToDto(Movie m) => new()
    {
        Id = m.Id,
        Title = m.Title,
        Description = m.Description,
        Genre = m.Genre,
        DurationMinutes = m.DurationMinutes,
        Rating = m.Rating,
        ShowtimeCount = m.Showtimes?.Count ?? 0
    };
}