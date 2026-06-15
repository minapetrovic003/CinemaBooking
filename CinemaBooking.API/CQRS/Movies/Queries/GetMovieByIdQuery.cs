using CinemaBooking.API.DTOs.Movies;
using MediatR;

namespace CinemaBooking.API.CQRS.Movies.Queries;

public record GetMovieByIdQuery(long Id) : IRequest<MovieDto?>;