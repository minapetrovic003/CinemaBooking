using CinemaBooking.Application.CQRS.Movies.Commands;
using CinemaBooking.Application.CQRS.Movies.Queries;
using CinemaBooking.Domain.DTOs.Movies;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.API.Controllers;

[ApiController]
[Route("movies")]
public class MoviesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MoviesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] MovieSearchRequest request)
    {
        var query = new GetAllMoviesQuery(
            request.Title,
            request.Genre,
            request.MinRating,
            request.Page,
            request.PageSize);

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _mediator.Send(new GetMovieByIdQuery(id));
        return result is null
            ? NotFound(new { Message = $"Movie with id {id} not found." })
            : Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMovieRequest request)
    {
        try
        {
            var command = new CreateMovieCommand(
                request.Title,
                request.Description,
                request.Genre,
                request.DurationMinutes,
                request.Rating);

            var movie = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = movie.Id }, movie);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateMovieRequest request)
    {
        try
        {
            var command = new UpdateMovieCommand(
                id,
                request.Title,
                request.Description,
                request.Genre,
                request.DurationMinutes,
                request.Rating);

            return await _mediator.Send(command)
                ? NoContent()
                : NotFound(new { Message = $"Movie with id {id} not found." });
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
        return await _mediator.Send(new DeleteMovieCommand(id))
            ? NoContent()
            : NotFound(new { Message = $"Movie with id {id} not found." });
    }
}