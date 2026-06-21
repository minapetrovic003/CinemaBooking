using CinemaBooking.Domain.DTOs.Halls;
using CinemaBooking.Application.Services;    // <-- izmenjeno
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.API.Controllers;

[ApiController]
[Route("halls")]
[Authorize(Roles = "Admin")]
public class HallsController : ControllerBase
{
    private readonly IHallService _hallService;
    private readonly IValidator<CreateHallRequest> _validator;

    public HallsController(IHallService hallService, IValidator<CreateHallRequest> validator)
    {
        _hallService = hallService;
        _validator = validator;
    }

    [HttpGet]
    public IActionResult GetAll() => Ok(_hallService.GetAll());

    [HttpGet("{id}")]
    public IActionResult GetById(long id)
    {
        var hall = _hallService.GetById(id);
        return hall is null
            ? NotFound(new { Message = $"Hall with id {id} not found." })
            : Ok(hall);
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateHallRequest request)
    {
        var v = _validator.Validate(request);
        if (!v.IsValid)
            return BadRequest(v.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        var (dto, conflict) = _hallService.Create(request);
        if (conflict is not null)
            return Conflict(new { Message = conflict });

        return CreatedAtAction(nameof(GetById), new { id = dto!.Id }, dto);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(long id)
        => _hallService.Delete(id)
            ? NoContent()
            : NotFound(new { Message = $"Hall with id {id} not found." });
}