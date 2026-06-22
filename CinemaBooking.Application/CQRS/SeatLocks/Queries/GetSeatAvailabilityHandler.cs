using CinemaBooking.Application.CQRS.SeatLocks.Queries;
using CinemaBooking.Application.Repositories;
using CinemaBooking.Domain.DTOs.SeatLocks;
using CinemaBooking.Domain.Models;
using MediatR;

namespace CinemaBooking.Application.CQRS.SeatLocks.Handlers;

public class GetSeatAvailabilityHandler
    : IRequestHandler<GetSeatAvailabilityQuery,
        (IEnumerable<SeatAvailabilityDto>? Result, string? ErrorMessage, int StatusCode)>
{
    private readonly IUnitOfWork _uow;

    public GetSeatAvailabilityHandler(IUnitOfWork uow) => _uow = uow;

    public Task<(IEnumerable<SeatAvailabilityDto>? Result, string? ErrorMessage, int StatusCode)> Handle(
        GetSeatAvailabilityQuery request, CancellationToken cancellationToken)
    {
        var showtime = _uow.Showtimes.GetByIdWithDetails(request.ShowtimeId);
        if (showtime is null)
            return Task.FromResult<(IEnumerable<SeatAvailabilityDto>?, string?, int)>(
                (null, $"Showtime {request.ShowtimeId} not found.", 404));

        var bookedSeatIds = _uow.Bookings.GetBookedSeatIds(request.ShowtimeId).ToHashSet();
        var activeLocks = _uow.SeatLocks.GetActiveLocks(request.ShowtimeId).ToList();
        var allSeats = showtime.Hall?.Seats ?? new List<Seat>();

        var result = allSeats.Select(seat =>
        {
            var isBooked = bookedSeatIds.Contains(seat.Id);
            var lockEntry = activeLocks.FirstOrDefault(l => l.SeatId == seat.Id);
            var isLockedByOther = lockEntry is not null && lockEntry.UserId != request.CurrentUserId;
            var isLockedByMe = lockEntry is not null && lockEntry.UserId == request.CurrentUserId;

            return new SeatAvailabilityDto
            {
                SeatId = seat.Id,
                Label = seat.GetSeatLabel(),
                Row = seat.Row,
                Number = seat.Number,
                SeatType = seat.SeatType.ToString(),
                Status = isBooked ? "Booked"
                    : isLockedByOther ? "Locked"
                    : isLockedByMe ? "MyLock"
                    : "Available",
                ExpiresAt = lockEntry?.ExpiresAt,
                ExpiresInSeconds = lockEntry is not null
                    ? Math.Max(0, (int)(lockEntry.ExpiresAt - DateTime.UtcNow).TotalSeconds)
                    : null
            };
        }).ToList();

        return Task.FromResult<(IEnumerable<SeatAvailabilityDto>?, string?, int)>(
            (result, null, 200));
    }
}