using CinemaBooking.API.DTOs.SeatLocks;
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

    // Maksimalan broj minuta koji klijent može tražiti
    private const int MaxLockMinutes = 15;

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
    /// Privremeno zaključava sedišta za korisnika na max 15 minuta.
    /// Vraća vreme isteka i broj sekundi do isteka (za countdown timer).
    /// </summary>
    [HttpPost("lock")]
    public async Task<IActionResult> LockSeats([FromBody] LockSeatsRequest request)
    {
        // ── Validacija korisnika ─────────────────────────────────────────────
        var user = await _userManager.FindByEmailAsync(request.UserEmail);
        if (user is null)
            return NotFound(new { Message = $"User '{request.UserEmail}' not found." });

        // ── Tražimo showtime ─────────────────────────────────────────────────
        var showtime = _uow.Showtimes
            .GetByMovieTitleHallAndStartTime(
                request.MovieTitle, request.HallName, request.ShowtimeStartTime);

        if (showtime is null)
            return NotFound(new { Message = "Showtime not found." });

        if (showtime.StartTime <= DateTime.UtcNow)
            return Conflict(new { Message = "Cannot lock seats for past showtimes." });

        // ── Tražimo sedišta ──────────────────────────────────────────────────
        var requestedLabels = request.Seats.Select(s => s.ToUpper()).ToList();
        var seats = showtime.Hall.Seats
            .Where(s => requestedLabels.Contains(s.GetSeatLabel().ToUpper()))
            .ToList();

        var notFound = requestedLabels
            .Except(seats.Select(s => s.GetSeatLabel().ToUpper()))
            .ToList();

        if (notFound.Any())
            return BadRequest(new { Message = $"Seats not found: {string.Join(", ", notFound)}" });

        // ── Proveravamo da li su sedišta već rezervisana (potvrđene rezervacije) ──
        var bookedSeatIds = _uow.Bookings.GetBookedSeatIds(showtime.Id).ToList();
        var alreadyBooked = seats.Where(s => bookedSeatIds.Contains(s.Id)).ToList();

        if (alreadyBooked.Any())
            return Conflict(new
            {
                Message = $"Seats already booked: {string.Join(", ", alreadyBooked.Select(s => s.GetSeatLabel()))}"
            });

        // ── Proveravamo da li DRUGI korisnik drži aktivan lock ───────────────
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

        // ── Kreiramo lock-ove ────────────────────────────────────────────────
        // Oslobađamo prethodne lock-ove ovog korisnika za isti showtime
        // (korisnik menja selekciju sedišta)
        _uow.SeatLocks.ReleaseLocksForUser(showtime.Id, user.Id);

        int lockMinutes = Math.Clamp(request.LockMinutes, 1, MaxLockMinutes);
        var expiresAt = DateTime.UtcNow.AddMinutes(lockMinutes);

        var locks = seats.Select(s =>
            SeatLock.Create(s.Id, showtime.Id, user.Id, lockMinutes)).ToList();

        _uow.SeatLocks.LockSeats(locks);
        _uow.SaveChanges();

        _logger.LogInformation(
            "User {Email} locked seats [{Seats}] for showtime {ShowtimeId}, expires at {ExpiresAt}",
            user.Email, string.Join(", ", requestedLabels), showtime.Id, expiresAt);

        return Ok(new SeatLockDto
        {
            LockedSeats = seats.Select(s => s.GetSeatLabel()).ToList(),
            ExpiresAt = expiresAt,
            Message = $"Seats locked for {lockMinutes} minutes. Complete your booking before {expiresAt:HH:mm} UTC."
        });
    }

    /// <summary>
    /// Ručno oslobađa lock-ove korisnika za dato prikazivanje.
    /// Poziva se ako korisnik odustane od rezervacije.
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
    /// Vraća listu zaključanih sedišta za dato prikazivanje.
    /// Frontend može koristiti ovaj endpoint da u realnom vremenu ažurira prikaz sale.
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
