using CinemaBooking.API.DTOs.Common;
using CinemaBooking.API.DTOs.Movies;
using MediatR;

namespace CinemaBooking.API.CQRS.Movies.Queries;

public record GetAllMoviesQuery(
    string? Title,
    string? Genre,
    decimal? MinRating,
    int Page,
    int PageSize
) : IRequest<PagedResult<MovieDto>>;