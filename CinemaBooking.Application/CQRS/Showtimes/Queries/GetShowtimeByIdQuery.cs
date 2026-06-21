using CinemaBooking.Domain.DTOs.Showtimes;
using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace CinemaBooking.Application.CQRS.Showtimes.Queries
{
    public record GetShowtimeByIdQuery(long Id) : IRequest<ShowtimeDto?>;
    
}
