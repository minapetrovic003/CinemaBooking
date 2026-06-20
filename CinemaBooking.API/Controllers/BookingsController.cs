using CinemaBooking.Application.CQRS.Bookings.Commands;
using CinemaBooking.Application.CQRS.Bookings.Queries;
using CinemaBooking.Application.DTOs.Bookings;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.API.Controllers;

[ApiController]
[Route("bookings")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BookingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] BookingSearchRequest request)
    {
        var query = new GetAllBookingsQuery(
            request.UserEmail,
            request.Status,
            request.FromDate,
            request.ToDate,
            request.Page,
            request.PageSize);

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _mediator.Send(new GetBookingByIdQuery(id));
        return result is null
            ? NotFound(new { Message = $"Booking with id {id} not found." })
            : Ok(result);
    }

    /// <summary>
    /// Javni endpoint za verifikaciju rezervacije (skeniranje QR koda).
    /// Vraca osnovne informacije o rezervaciji — status, film, sedista.
    /// </summary>
    [HttpGet("{id}/verify")]
    [AllowAnonymous]
    public async Task<IActionResult> Verify(long id)
    {
        var result = await _mediator.Send(new GetBookingByIdQuery(id));
        if (result is null)
            return NotFound(new { Message = $"Booking {id} not found." });

        return Ok(new
        {
            BookingId = result.Id,
            Status = result.Status,
            Movie = result.MovieTitle,
            Hall = result.HallName,
            Showtime = result.ShowtimeStart,
            Seats = result.Seats.Select(s => s.SeatLabel),
            TotalPrice = result.TotalPrice,
            CustomerName = result.UserFullName
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest request)
    {
        try
        {
            var command = new CreateBookingCommand(
                request.UserEmail,
                request.MovieTitle,
                request.HallName,
                request.ShowtimeStartTime,
                request.Seats);

            var (dto, errorMessage, statusCode) = await _mediator.Send(command);

            return statusCode switch
            {
                404 => NotFound(new { Message = errorMessage }),
                409 => Conflict(new { Message = errorMessage }),
                400 => BadRequest(new { Message = errorMessage }),
                201 => CreatedAtAction(nameof(GetById), new { id = dto!.Id }, dto),
                _ => StatusCode(statusCode, new { Message = errorMessage })
            };
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
        }
    }

    [HttpPatch("{id}/cancel")]
    public async Task<IActionResult> Cancel(long id)
    {
        var existing = await _mediator.Send(new GetBookingByIdQuery(id));
        if (existing is null)
            return NotFound(new { Message = $"Booking with id {id} not found." });

        var (success, errorMessage) = await _mediator.Send(new CancelBookingCommand(id));
        return success
            ? NoContent()
            : Conflict(new { Message = errorMessage });
    }

    /// <summary>
    /// Check-in pri ulasku u bioskop — poziva se nakon skeniranja QR koda (samo Admin).
    /// Mijenja status rezervacije iz Confirmed → CheckedIn.
    /// </summary>
    [HttpPatch("{id}/checkin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CheckIn(long id)
    {
        var existing = await _mediator.Send(new GetBookingByIdQuery(id));
        if (existing is null)
            return NotFound(new { Message = $"Booking with id {id} not found." });

        var (success, errorMessage) = await _mediator.Send(new CheckInBookingCommand(id));
        return success
            ? NoContent()
            : Conflict(new { Message = errorMessage });
    }
}