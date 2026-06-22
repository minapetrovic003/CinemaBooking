using CinemaBooking.Application.CQRS.SeatLocks.Commands;
using CinemaBooking.Application.Repositories;
using CinemaBooking.Domain.DTOs.SeatLocks;
using CinemaBooking.Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CinemaBooking.Application.CQRS.SeatLocks.Handlers;

public class LockSeatsHandler
    : IRequestHandler<LockSeatsCommand, (SeatLockDto? Result, string? ErrorMessage, int StatusCode)>
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<LockSeatsHandler> _logger;

    private const int MaxLockMinutes = 15;

    private static readonly TimeZoneInfo BelgradeZone =
        TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "Central European Standard Time" : "Europe/Belgrade");

    public LockSeatsHandler(IUnitOfWork uow, ILogger<LockSeatsHandler> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<(SeatLockDto? Result, string? ErrorMessage, int StatusCode)> Handle(
        LockSeatsCommand request, CancellationToken cancellationToken)
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

        var bookedSeatIds = _uow.Bookings.GetBookedSeatIds(showtime.Id).ToList();
        var alreadyBooked = seats.Where(s => bookedSeatIds.Contains(s.Id)).ToList();

        if (alreadyBooked.Any())
            return (null, $"Seats already booked: {string.Join(", ", alreadyBooked.Select(s => s.GetSeatLabel()))}", 409);

        var seatIds = seats.Select(s => s.Id).ToList();
        var lockedByOthers = _uow.SeatLocks
            .GetActiveLocksForSeats(showtime.Id, seatIds)
            .Where(sl => !sl.OwnedBy(request.UserId))
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

        // Oslobodi prethodne lock-ove ovog korisnika (promena selekcije)
        _uow.SeatLocks.ReleaseLocksForUser(showtime.Id, request.UserId);

        int lockMinutes = Math.Clamp(request.LockMinutes, 1, MaxLockMinutes);
        var expiresAt = DateTime.UtcNow.AddMinutes(lockMinutes);

        var locks = seats.Select(s =>
            SeatLock.Create(s.Id, showtime.Id, request.UserId, lockMinutes)).ToList();

        _uow.SeatLocks.LockSeats(locks);
        await _uow.SaveChangesAsync();

        var expiresAtBelgrade = TimeZoneInfo.ConvertTimeFromUtc(expiresAt, BelgradeZone);

        _logger.LogInformation(
            "User {UserId} locked seats [{Seats}] for showtime {ShowtimeId}, expires at {ExpiresAt} UTC.",
            request.UserId, string.Join(", ", requestedLabels), showtime.Id, expiresAt);

        return (new SeatLockDto
        {
            LockedSeats = seats.Select(s => s.GetSeatLabel()).ToList(),
            ExpiresAt = expiresAt,
            Message = $"Seats locked for {lockMinutes} minutes. " +
                      $"Complete your booking before {expiresAtBelgrade:HH:mm} (Belgrade time)."
        }, null, 200);
    }
}