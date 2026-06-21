using CinemaBooking.Domain.DTOs.Halls;
using CinemaBooking.Domain.DTOs.Movies;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaBooking.Application.CQRS.Halls.Queries
{
    public record GetHallByIdQuery(long id) : IRequest<HallDto?>;
}
