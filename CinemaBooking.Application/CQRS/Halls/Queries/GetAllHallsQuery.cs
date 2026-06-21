using CinemaBooking.Domain.DTOs.Halls;
using MediatR;

namespace CinemaBooking.Application.CQRS.Halls.Queries
{
    public record GetAllHallsQuery() : IRequest<List<HallDto>>;
}