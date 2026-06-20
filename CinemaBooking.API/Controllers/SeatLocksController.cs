using CinemaBooking.Application.DTOs.SeatLocks;
using CinemaBooking.Domain;
using CinemaBooking.Domain.Repositories;
using CinemaBooking.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.API.Controllers;

[ApiController]
[Route("seat-locks")]
[Authorize]
public class SeatLocksController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<SeatLocksController> _logger;

    private const int MaxLockMinutes = 15;

    // Belgrade timezone for user-facing time display
    private static readonly TimeZoneInfo BelgradeZone =
        TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "Central European Standard Time" : "Europe/Belgrade");

    public SeatLocksController(
        IUnitOfWork uow,
        UserManager<ApplicationUser> userManager,
        ILogger<SeatLocksController> logger)
    {
        _uow = uow;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Returns consolidated seat availability for a showtime.
    /// Frontend polls this every few seconds to keep the seat map current.
    /// Returns: seatId, status (Available/Booked/Locked), lockedByCurrentUser
    /// </summary>
    [HttpGet("availability/{showtimeId:long}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvailability(long showtimeId)
    {
        var showtime = _uow.Showtimes.GetByIdWithDetails(showtimeId);
        if (showtime is null)
            return NotFound(new { Message = $"Showtime {showtimeId} not found." });

        // Get current user id if authenticated (to mark their own locks)
        string? currentUserId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                     ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
            if (email is not null)
            {
                var u = await _userManager.FindByEmailAsync(email);
                currentUserId = u?.Id;
            }
        }

        var bookedSeatIds = _uow.Bookings.GetBookedSeatIds(showtimeId).ToHashSet();
        var activeLocks = _uow.SeatLocks.GetActiveLocks(showtimeId).ToList();

        var allSeats = showtime.Hall?.Seats ?? new List<Seat>();

        var result = allSeats.Select(seat =>
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
        });

        return Ok(result);
    }

    /// <summary>
    /// Temporarily locks seats for the user for up to 15 minutes.
    /// Returns expiry time and seconds remaining (for countdown timer).
    /// </summary>
    [HttpPost("lock")]
    public async Task<IActionResult> LockSeats([FromBody] LockSeatsRequest request)
    {
        // Validate user
        var user = await _userManager.FindByEmailAsync(request.UserEmail);
        if (user is null)
            return NotFound(new { Message = $"User '{request.UserEmail}' not found." });

        // Find showtime
        var showtime = _uow.Showtimes
            .GetByMovieTitleHallAndStartTime(
                request.MovieTitle, request.HallName, request.ShowtimeStartTime);

        if (showtime is null)
            return NotFound(new { Message = "Showtime not found." });

        if (showtime.StartTime <= DateTime.UtcNow)
            return Conflict(new { Message = "Cannot lock seats for past showtimes." });

        // Find seats by label
        var requestedLabels = request.Seats.Select(s => s.ToUpper()).ToList();
        var seats = showtime.Hall.Seats
            .Where(s => requestedLabels.Contains(s.GetSeatLabel().ToUpper()))
            .ToList();

        var notFound = requestedLabels
            .Except(seats.Select(s => s.GetSeatLabel().ToUpper()))
            .ToList();

        if (notFound.Any())
            return BadRequest(new { Message = $"Seats not found: {string.Join(", ", notFound)}" });

        // Check if seats are already booked (confirmed bookings)
        var bookedSeatIds = _uow.Bookings.GetBookedSeatIds(showtime.Id).ToList();
        var alreadyBooked = seats.Where(s => bookedSeatIds.Contains(s.Id)).ToList();

        if (alreadyBooked.Any())
            return Conflict(new
            {
                Message = $"Seats already booked: {string.Join(", ", alreadyBooked.Select(s => s.GetSeatLabel()))}"
            });

        // Check if seats are locked by ANOTHER user
        var seatIds = seats.Select(s => s.Id).ToList();
        var lockedByOthers = _uow.SeatLocks
            .GetActiveLocksForSeats(showtime.Id, seatIds)
            .Where(sl => !sl.OwnedBy(user.Id))
            .ToList();

        if (lockedByOthers.Any())
        {
            var lockedLabels = seats
                .Where(s => lockedByOthers.Any(l => l.SeatId == s.Id))
                .Select(s => s.GetSeatLabel());

            return Conflict(new
            {
                Message = $"Seats temporarily locked by another user: {string.Join(", ", lockedLabels)}. " +
                          "Please try again in a few minutes."
            });
        }

        // Release previous locks by this user for this showtime (user changed seat selection)
        _uow.SeatLocks.ReleaseLocksForUser(showtime.Id, user.Id);

        int lockMinutes = Math.Clamp(request.LockMinutes, 1, MaxLockMinutes);
        var expiresAt = DateTime.UtcNow.AddMinutes(lockMinutes);

        var locks = seats.Select(s =>
            SeatLock.Create(s.Id, showtime.Id, user.Id, lockMinutes)).ToList();

        _uow.SeatLocks.LockSeats(locks);
        _uow.SaveChanges();

        // Convert expiry to Belgrade time for user-facing message
        var expiresAtBelgrade = TimeZoneInfo.ConvertTimeFromUtc(expiresAt, BelgradeZone);

        _logger.LogInformation(
            "User {Email} locked seats [{Seats}] for showtime {ShowtimeId}, expires at {ExpiresAt} UTC",
            user.Email, string.Join(", ", requestedLabels), showtime.Id, expiresAt);

        return Ok(new SeatLockDto
        {
            LockedSeats = seats.Select(s => s.GetSeatLabel()).ToList(),
            ExpiresAt = expiresAt,
            Message = $"Seats locked for {lockMinutes} minutes. Complete your booking before {expiresAtBelgrade:HH:mm} (Belgrade time)."
        });
    }

    /// <summary>
    /// Manually releases the current user's locks for a given showtime.
    /// Call when user clicks Back/Cancel.
    /// </summary>
    [HttpDelete("release")]
    public async Task<IActionResult> ReleaseLocks([FromQuery] string userEmail, [FromQuery] long showtimeId)
    {
        var user = await _userManager.FindByEmailAsync(userEmail);
        if (user is null)
            return NotFound(new { Message = $"User '{userEmail}' not found." });

        _uow.SeatLocks.ReleaseLocksForUser(showtimeId, user.Id);
        _uow.SaveChanges();

        return NoContent();
    }

    /// <summary>
    /// Returns active lock seatIds for a showtime (legacy endpoint, kept for compatibility).
    /// Prefer /availability/{showtimeId} for full seat status.
    /// </summary>
    [HttpGet("status/{showtimeId:long}")]
    public IActionResult GetLockStatus(long showtimeId)
    {
        var activeLocks = _uow.SeatLocks.GetActiveLocks(showtimeId);

        var result = activeLocks.Select(sl => new
        {
            SeatId = sl.SeatId,
            ExpiresAt = sl.ExpiresAt,
            ExpiresInSeconds = Math.Max(0, (int)(sl.ExpiresAt - DateTime.UtcNow).TotalSeconds)
        });

        return Ok(result);
    }
}