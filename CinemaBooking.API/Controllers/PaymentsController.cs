using CinemaBooking.Application.CQRS.Payments.Commands;
using CinemaBooking.Application.CQRS.Payments.Queries;
using CinemaBooking.Domain.DTOs.Payments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.API.Controllers;

[ApiController]
[Route("payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _mediator.Send(new GetAllPaymentsQuery()));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var payment = await _mediator.Send(new GetPaymentByIdQuery(id));
        return payment is null
            ? NotFound(new { Message = $"Payment with id {id} not found." })
            : Ok(payment);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePaymentRequest request)
    {
        var command = new CreatePaymentCommand(
            request.UserEmail,
            request.MovieTitle,
            request.HallName,
            request.ShowtimeStartTime,
            request.Method);

        var (dto, errorMessage, statusCode) = await _mediator.Send(command);

        return statusCode switch
        {
            400 => BadRequest(new { Message = errorMessage }),
            404 => NotFound(new { Message = errorMessage }),
            409 => Conflict(new { Message = errorMessage }),
            201 => CreatedAtAction(nameof(GetById), new { id = dto!.Id }, dto),
            _ => StatusCode(statusCode, new { Message = errorMessage })
        };
    }

    [HttpPatch("{id}/refund")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Refund(long id)
    {
        var existing = await _mediator.Send(new GetPaymentByIdQuery(id));
        if (existing is null)
            return NotFound(new { Message = $"Payment with id {id} not found." });

        var (success, errorMessage) = await _mediator.Send(new RefundPaymentCommand(id));
        return success
            ? NoContent()
            : Conflict(new { Message = errorMessage });
    }
}