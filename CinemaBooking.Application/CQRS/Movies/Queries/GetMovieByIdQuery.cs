using CinemaBooking.Application.DTOs.Movies;
using MediatR;

namespace CinemaBooking.Application.CQRS.Movies.Queries;

public record GetMovieByIdQuery(long Id) : IRequest<MovieDto?>;