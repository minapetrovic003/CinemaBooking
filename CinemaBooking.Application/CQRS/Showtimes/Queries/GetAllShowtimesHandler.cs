using CinemaBooking.Application.Repositories;
using CinemaBooking.Domain.DTOs.Showtimes;
using CinemaBooking.Domain.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaBooking.Application.CQRS.Showtimes.Queries
{
    public class GetAllShowtimesHandler : IRequestHandler<GetAllShowtimesQuery, IEnumerable<ShowtimeDto>>
    {
        private readonly IUnitOfWork _uow;

        public GetAllShowtimesHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public Task<IEnumerable<ShowtimeDto>> Handle(GetAllShowtimesQuery request, CancellationToken cancellationToken)
        {
            var result = _uow.Showtimes.Search(request.movieTitle, request.fromDate).Select(MapToDto);
            return Task.FromResult(result);
        }

        internal static ShowtimeDto MapToDto(Showtime s)
        {
            var confirmed = s.Bookings?
                .Where(b => b.Status != BookingStatus.Canceled).ToList()
                ?? new List<Booking>();

            var bookedSeats = confirmed.SelectMany(b => b.BookingSeats).Count();

            return new ShowtimeDto
            {
                Id = s.Id,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                Price = s.Price,
                MovieTitle = s.Movie?.Title ?? string.Empty,
                MovieGenre = s.Movie?.Genre ?? string.Empty,
                MovieDurationMinutes = s.Movie?.DurationMinutes ?? 0,
                HallName = s.Hall?.Name ?? string.Empty,
                HallCapacity = s.Hall?.Capacity ?? 0,
                BookingCount = confirmed.Count,
                AvailableSeats = (s.Hall?.Capacity ?? 0) - bookedSeats
            };
        }
    }
}
