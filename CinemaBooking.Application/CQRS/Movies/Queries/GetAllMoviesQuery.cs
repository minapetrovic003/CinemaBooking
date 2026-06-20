using CinemaBooking.Application.DTOs.Common;
using CinemaBooking.Application.DTOs.Movies;
using MediatR;

namespace CinemaBooking.Application.CQRS.Movies.Queries;

public record GetAllMoviesQuery(
    string? Title,
    string? Genre,
    decimal? MinRating,
    int Page,
    int PageSize
) : IRequest<PagedResult<MovieDto>>;