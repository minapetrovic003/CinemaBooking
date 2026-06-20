using CinemaBooking.Application.DTOs.SeatLocks;
using CinemaBooking.Application.Services;
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
    private readonly ISeatLockService _seatLockService;
    private readonly UserManager<ApplicationUser> _userManager;

    public SeatLocksController(
        ISeatLockService seatLockService,
        UserManager<ApplicationUser> userManager)
    {
        _seatLockService = seatLockService;
        _userManager = userManager;
    }

    /// <summary>
    /// Vraca konsolidovanu mapu dostupnosti sedista za datu projekciju.
    /// Frontend polluje ovaj endpoint svakih 5 sekundi.
    /// </summary>
    [HttpGet("availability/{showtimeId:long}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvailability(long showtimeId)
    {
        // Resolve current user ID (null ako nije ulogovan)
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

        var result = _seatLockService.GetAvailability(showtimeId, currentUserId);

        return result is null
            ? NotFound(new { Message = $"Showtime {showtimeId} not found." })
            : Ok(result);
    }

    /// <summary>
    /// Zakljucava odabrana sedista za korisnika (max 15 min).
    /// </summary>
    [HttpPost("lock")]
    public async Task<IActionResult> LockSeats([FromBody] LockSeatsRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.UserEmail);
        if (user is null)
            return NotFound(new { Message = $"User '{request.UserEmail}' not found." });

        var (result, error, statusCode) = _seatLockService.LockSeats(request, user.Id);

        return statusCode switch
        {
            200 => Ok(result),
            400 => BadRequest(new { Message = error }),
            404 => NotFound(new { Message = error }),
            409 => Conflict(new { Message = error }),
            _ => StatusCode(statusCode, new { Message = error })
        };
    }

    /// <summary>
    /// Oslobadja lock-ove korisnika kada klikne "Nazad" ili zatvori modal.
    /// </summary>
    [HttpDelete("release")]
    public async Task<IActionResult> ReleaseLocks(
        [FromQuery] string userEmail, [FromQuery] long showtimeId)
    {
        var user = await _userManager.FindByEmailAsync(userEmail);
        if (user is null)
            return NotFound(new { Message = $"User '{userEmail}' not found." });

        _seatLockService.ReleaseLocks(user.Id, showtimeId);
        return NoContent();
    }

    /// <summary>
    /// Legacy endpoint — vraca aktivne lock-ove za projekciju.
    /// Preferuj /availability/{showtimeId} za potpunu sliku.
    /// </summary>
    [HttpGet("status/{showtimeId:long}")]
    public IActionResult GetLockStatus(long showtimeId)
    {
        var availability = _seatLockService.GetAvailability(showtimeId, currentUserId: null);
        if (availability is null)
            return NotFound(new { Message = $"Showtime {showtimeId} not found." });

        var locked = availability
            .Where(s => s.Status == "Locked" || s.Status == "MyLock")
            .Select(s => new
            {
                SeatId = s.SeatId,
                Label = s.Label,
                ExpiresAt = s.ExpiresAt,
                ExpiresInSeconds = s.ExpiresInSeconds
            });

        return Ok(locked);
    }
}