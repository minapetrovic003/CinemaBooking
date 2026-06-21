using CinemaBooking.Domain.DTOs.SeatLocks;
using CinemaBooking.Application.Services;
using CinemaBooking.Domain.Models;
using CinemaBooking.Application.Repositories;
using Microsoft.Extensions.Logging;

namespace CinemaBooking.Application.Notifications;

// Sve poslovne logike lokovanja sedista — izvuceno iz SeatLocksController
public class SeatLockService : ISeatLockService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<SeatLockService> _logger;

    private const int MaxLockMinutes = 15;

    private static readonly TimeZoneInfo BelgradeZone =
        TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "Central European Standard Time" : "Europe/Belgrade");

    public SeatLockService(IUnitOfWork uow, ILogger<SeatLockService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public IEnumerable<SeatAvailabilityDto>? GetAvailability(long showtimeId, string? currentUserId)
    {
        var showtime = _uow.Showtimes.GetByIdWithDetails(showtimeId);
        if (showtime is null) return null;

        var bookedSeatIds = _uow.Bookings.GetBookedSeatIds(showtimeId).ToHashSet();
        var activeLocks = _uow.SeatLocks.GetActiveLocks(showtimeId).ToList();
        var allSeats = showtime.Hall?.Seats ?? new List<Seat>();

        return allSeats.Select(seat =>
        {
            var isBooked = bookedSeatIds.Contains(seat.Id);
            var lockEntry = activeLocks.FirstOrDefault(l => l.SeatId == seat.Id);
            var isLockedByOther = lockEntry is not null && lockEntry.UserId != currentUserId;
            var isLockedByMe = lockEntry is not null && lockEntry.UserId == currentUserId;

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
    }

    public (SeatLockDto? Result, string? Error, int StatusCode) LockSeats(
        LockSeatsRequest request, string userId)
    {
        var showtime = _uow.Showtimes
            .GetByMovieTitleHallAndStartTime(
                request.MovieTitle, request.HallName, request.ShowtimeStartTime);

        if (showtime is null)
            return (null, "Showtime not found.", 404);

        if (showtime.StartTime <= DateTime.UtcNow)
            return (null, "Cannot lock seats for past showtimes.", 409);

        var requestedLabels = request.Seats.Select(s => s.ToUpper()).ToList();
        var seats = showtime.Hall.Seats
            .Where(s => requestedLabels.Contains(s.GetSeatLabel().ToUpper()))
            .ToList();

        var notFound = requestedLabels
            .Except(seats.Select(s => s.GetSeatLabel().ToUpper()))
            .ToList();

        if (notFound.Any())
            return (null, $"Seats not found: {string.Join(", ", notFound)}", 400);

        // Provjera da li su sedista vec rezervisana
        var bookedSeatIds = _uow.Bookings.GetBookedSeatIds(showtime.Id).ToList();
        var alreadyBooked = seats.Where(s => bookedSeatIds.Contains(s.Id)).ToList();

        if (alreadyBooked.Any())
            return (null,
                $"Seats already booked: {string.Join(", ", alreadyBooked.Select(s => s.GetSeatLabel()))}",
                409);

        // Provjera da li su sedista zakljucana od strane DRUGOG korisnika
        var seatIds = seats.Select(s => s.Id).ToList();
        var lockedByOthers = _uow.SeatLocks
            .GetActiveLocksForSeats(showtime.Id, seatIds)
            .Where(sl => !sl.OwnedBy(userId))
            .ToList();

        if (lockedByOthers.Any())
        {
            var lockedLabels = seats
                .Where(s => lockedByOthers.Any(l => l.SeatId == s.Id))
                .Select(s => s.GetSeatLabel());

            return (null,
                $"Seats temporarily locked by another user: {string.Join(", ", lockedLabels)}. " +
                "Please try again in a few minutes.",
                409);
        }

        // Oslobodi prethodne lock-ove ovog korisnika (promjena selekcije)
        _uow.SeatLocks.ReleaseLocksForUser(showtime.Id, userId);

        int lockMinutes = Math.Clamp(request.LockMinutes, 1, MaxLockMinutes);
        var expiresAt = DateTime.UtcNow.AddMinutes(lockMinutes);

        var locks = seats.Select(s =>
            SeatLock.Create(s.Id, showtime.Id, userId, lockMinutes)).ToList();

        _uow.SeatLocks.LockSeats(locks);
        _uow.SaveChanges();

        var expiresAtBelgrade = TimeZoneInfo.ConvertTimeFromUtc(expiresAt, BelgradeZone);

        _logger.LogInformation(
            "User {UserId} locked seats [{Seats}] for showtime {ShowtimeId}, expires at {ExpiresAt} UTC.",
            userId, string.Join(", ", requestedLabels), showtime.Id, expiresAt);

        return (new SeatLockDto
        {
            LockedSeats = seats.Select(s => s.GetSeatLabel()).ToList(),
            ExpiresAt = expiresAt,
            Message = $"Seats locked for {lockMinutes} minutes. " +
                      $"Complete your booking before {expiresAtBelgrade:HH:mm} (Belgrade time)."
        }, null, 200);
    }

    public (bool Success, string? Error) ReleaseLocks(string userId, long showtimeId)
    {
        _uow.SeatLocks.ReleaseLocksForUser(showtimeId, userId);
        _uow.SaveChanges();
        return (true, null);
    }
}