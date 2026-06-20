using CinemaBooking.Application.DTOs.Notifications;
using CinemaBooking.Application.Services.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.API.Controllers;

[ApiController]
[Route("notifications")]
[Authorize]   // Ukloni Authorize i stavi [AllowAnonymous] samo privremeno za testiranje
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationService notificationService,
        ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Test endpoint — provjeri da li SMTP konfiguracija radi.
    /// Zahtijeva JWT token. Za brzi test bez tokena zamijeni [Authorize] sa [AllowAnonymous].
    /// </summary>
    [HttpPost("test-email")]
    public async Task<IActionResult> SendTestEmail(
        [FromBody] TestEmailRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ToEmail))
            return BadRequest(new { Message = "ToEmail je obavezan." });

        try
        {
            await _notificationService.SendEmailAsync(
                request.ToEmail,
                request.ToName,
                request.Subject,
                $"<p>{request.Body}</p>",
                cancellationToken);

            return Ok(new { Message = $"Test email uspješno poslan na {request.ToEmail}." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test email nije poslan na {Email}", request.ToEmail);
            return StatusCode(500, new { Message = "Slanje emaila nije uspjelo.", Detail = ex.Message });
        }
    }
}