using CinemaBooking.Domain.DTOs.Payments;
using CinemaBooking.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.API.Controllers;

[ApiController]
[Route("payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
        => _paymentService = paymentService;

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult GetAll() => Ok(_paymentService.GetAll());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var payment = await _paymentService.GetByIdAsync(id);
        return payment is null
            ? NotFound(new { Message = $"Payment with id {id} not found." })
            : Ok(payment);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePaymentRequest request)
    {
        var (dto, errorMessage, statusCode) = await _paymentService.CreateAsync(request);

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
        if (await _paymentService.GetByIdAsync(id) is null)
            return NotFound(new { Message = $"Payment with id {id} not found." });

        var (success, errorMessage) = await _paymentService.RefundAsync(id);
        return success
            ? NoContent()
            : Conflict(new { Message = errorMessage });
    }
}