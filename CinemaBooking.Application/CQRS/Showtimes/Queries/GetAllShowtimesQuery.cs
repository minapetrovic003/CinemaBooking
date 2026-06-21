using System;
using System.Collections.Generic;
using System.Text;
using CinemaBooking.Domain.DTOs.Showtimes;
using MediatR;

namespace CinemaBooking.Application.CQRS.Showtimes.Queries
{
    public record GetAllShowtimesQuery(string? movieTitle, DateTime? fromDate) : IRequest<IEnumerable<ShowtimeDto>>;
    
    
}
