using CinemaBooking.Application.CQRS.SeatLocks.Commands;
using CinemaBooking.Application.CQRS.SeatLocks.Queries;
using CinemaBooking.Domain.DTOs.SeatLocks;
using CinemaBooking.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.API.Controllers;

[ApiController]
[Route("seat-locks")]
[Authorize]
public class SeatLocksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly UserManager<ApplicationUser> _userManager;

    public SeatLocksController(IMediator mediator, UserManager<ApplicationUser> userManager)
    {
        _mediator = mediator;
        _userManager = userManager;
    }

    
    [HttpGet("availability/{showtimeId:long}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvailability(long showtimeId)
    {
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

        var (result, errorMessage, statusCode) =
            await _mediator.Send(new GetSeatAvailabilityQuery(showtimeId, currentUserId));

        return statusCode switch
        {
            200 => Ok(result),
            404 => NotFound(new { Message = errorMessage }),
            _ => StatusCode(statusCode, new { Message = errorMessage })
        };
    }


    [HttpPost("lock")]
    public async Task<IActionResult> LockSeats([FromBody] LockSeatsRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.UserEmail);
        if (user is null)
            return NotFound(new { Message = $"User '{request.UserEmail}' not found." });

        var command = new LockSeatsCommand(
            UserId: user.Id,
            MovieTitle: request.MovieTitle,
            HallName: request.HallName,
            ShowtimeStartTime: request.ShowtimeStartTime,
            Seats: request.Seats,
            LockMinutes: request.LockMinutes);

        var (result, errorMessage, statusCode) = await _mediator.Send(command);

        return statusCode switch
        {
            200 => Ok(result),
            400 => BadRequest(new { Message = errorMessage }),
            404 => NotFound(new { Message = errorMessage }),
            409 => Conflict(new { Message = errorMessage }),
            _ => StatusCode(statusCode, new { Message = errorMessage })
        };
    }

 
    [HttpDelete("release")]
    public async Task<IActionResult> ReleaseLocks(
        [FromQuery] string userEmail, [FromQuery] long showtimeId)
    {
        var user = await _userManager.FindByEmailAsync(userEmail);
        if (user is null)
            return NotFound(new { Message = $"User '{userEmail}' not found." });

        var (success, errorMessage) =
            await _mediator.Send(new ReleaseLocksCommand(user.Id, showtimeId));

        return success ? NoContent() : BadRequest(new { Message = errorMessage });
    }
}