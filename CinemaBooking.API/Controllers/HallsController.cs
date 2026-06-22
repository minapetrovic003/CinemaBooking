using CinemaBooking.Application.CQRS.Halls.Commands;
using CinemaBooking.Application.CQRS.Halls.Queries;
using CinemaBooking.Domain.DTOs.Halls;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.API.Controllers;

[ApiController]
[Route("halls")]
[Authorize(Roles = "Admin")]
public class HallsController : ControllerBase
{
    private readonly IMediator _mediator;

    public HallsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllHallsQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var hall = await _mediator.Send(new GetHallByIdQuery(id));
        return hall is null
            ? NotFound(new { Message = $"Hall with id {id} not found." })
            : Ok(hall);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateHallRequest request)
    {
        try
        {
            var command = new CreateHallCommand(
                request.Name, request.Capacity,
                request.Rows, request.SeatsPerRow);

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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        return await _mediator.Send(new DeleteHallCommand(id))
            ? NoContent()
            : NotFound(new { Message = $"Hall with id {id} not found." });
    }

    
    [HttpPatch("{hallId}/seats/{seatId}/type")]
    public async Task<IActionResult> UpdateSeatType(
        long hallId, long seatId, [FromBody] UpdateSeatTypeRequest request)
    {
        var command = new UpdateSeatTypeCommand(hallId, seatId, request.SeatType);
        var (success, errorMessage, statusCode) = await _mediator.Send(command);

        return statusCode switch
        {
            204 => NoContent(),
            400 => BadRequest(new { Message = errorMessage }),
            404 => NotFound(new { Message = errorMessage }),
            _ => StatusCode(statusCode, new { Message = errorMessage })
        };
    }
}