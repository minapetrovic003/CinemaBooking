using CinemaBooking.API.DTOs.Showtimes;
using CinemaBooking.API.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.API.Controllers;

[ApiController]
[Route("showtimes")]
public class ShowtimesController : ControllerBase
{
    private readonly IShowtimeService _showtimeService;
    private readonly IValidator<CreateShowtimeRequest> _validator;

    public ShowtimesController(IShowtimeService showtimeService, IValidator<CreateShowtimeRequest> validator)
    {
        _showtimeService = showtimeService;
        _validator = validator;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult GetAll([FromQuery] string? movieTitle, [FromQuery] DateTime? fromDate)
        => Ok(_showtimeService.GetAll(movieTitle, fromDate));

    [AllowAnonymous]
    [HttpGet("{id}")]
    public IActionResult GetById(long id)
    {
        var s = _showtimeService.GetById(id);
        return s is null
            ? NotFound(new { Message = $"Showtime with id {id} not found." })
            : Ok(s);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public IActionResult Create([FromBody] CreateShowtimeRequest request)
    {
        var v = _validator.Validate(request);
        if (!v.IsValid)
            return BadRequest(v.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        var (dto, errorMessage, statusCode) = _showtimeService.Create(request);

        return statusCode switch
        {
            404 => NotFound(new { Message = errorMessage }),
            409 => Conflict(new { Message = errorMessage }),
            201 => CreatedAtAction(nameof(GetById), new { id = dto!.Id }, dto),
            _ => StatusCode(statusCode, new { Message = errorMessage })
        };
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public IActionResult Delete(long id)
        => _showtimeService.Delete(id)
            ? NoContent()
            : NotFound(new { Message = $"Showtime with id {id} not found." });
}