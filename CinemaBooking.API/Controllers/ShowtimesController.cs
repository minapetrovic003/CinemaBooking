using CinemaBooking.Application.CQRS.Showtimes.Commands;
using CinemaBooking.Application.CQRS.Showtimes.Queries;
using CinemaBooking.Domain.DTOs.Showtimes;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.API.Controllers;

[ApiController]
[Route("showtimes")]
public class ShowtimesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ShowtimesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? movieTitle, [FromQuery] DateTime? fromDate)
    {
        var result = await _mediator.Send(new GetAllShowtimesQuery(movieTitle, fromDate));
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _mediator.Send(new GetShowtimeByIdQuery(id));
        return result is null
            ? NotFound(new { Message = $"Showtime with id {id} not found." })
            : Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateShowtimeRequest request)
    {
        try
        {
            var command = new CreateShowtimeCommand(
                request.MovieTitle,
                request.HallName,
                request.StartTime,
                request.Price);

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

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        return await _mediator.Send(new DeleteShowtimeCommand(id))
            ? NoContent()
            : NotFound(new { Message = $"Showtime with id {id} not found." });
    }
}