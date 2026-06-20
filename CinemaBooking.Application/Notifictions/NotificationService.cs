using CinemaBooking.Application.Services.Notifications;
using CinemaBooking.Domain;
using CinemaBooking.Infrastructure.Identity;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace CinemaBooking.API.Services.Notifications;

public class NotificationService : INotificationService
{
    private readonly SmtpSettings _smtp;
    private readonly ILogger<NotificationService> _logger;

    private static readonly TimeZoneInfo BelgradeZone =
        TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "Central European Standard Time" : "Europe/Belgrade");

    public NotificationService(
        IOptions<SmtpSettings> smtpOptions,
        ILogger<NotificationService> logger)
    {
        _smtp = smtpOptions.Value;
        _logger = logger;
    }

    private static string ToBelgrade(DateTime utcTime)
    {
        var local = TimeZoneInfo.ConvertTimeFromUtc(utcTime, BelgradeZone);
        return local.ToString("dd.MM.yyyy HH:mm");
    }

    public async Task SendEmailAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_smtp.FromName, _smtp.FromEmail));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;
        message.Body = new TextPart(TextFormat.Html) { Text = htmlBody };

        using var client = new MailKit.Net.Smtp.SmtpClient();

        await client.ConnectAsync(
            _smtp.Host,
            _smtp.Port,
            _smtp.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None,
            cancellationToken);

        await client.AuthenticateAsync(_smtp.UserName, _smtp.Password, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        _logger.LogInformation(
            "Email sent | Subject: '{Subject}' | To: {Email}",
            subject, toEmail);
    }

    public async Task SendBookingConfirmationAsync(
        Booking booking,
        ApplicationUser user,
        CancellationToken cancellationToken = default)
    {
        var seatRows = booking.BookingSeats
            .Select(bs =>
                $"<tr><td>{bs.GetSeatLabel()}</td><td>{bs.Seat?.SeatType}</td><td style=\"text-align:right\">&euro;{bs.Price:F2}</td></tr>")
            .ToList();

        var showtimeLocal = booking.Showtime?.StartTime is not null
            ? ToBelgrade(booking.Showtime.StartTime)
            : "N/A";

        var html = $"""
            <div style="font-family:Arial,sans-serif;max-width:600px;margin:0 auto">
              <h2 style="color:#1a1a2e">&#x2705; Booking Confirmation</h2>
              <p>Dear <strong>{user.GetFullName()}</strong>,</p>
              <p>Your booking has been successfully created. Here are your details:</p>
              <table border="1" cellpadding="8" style="border-collapse:collapse;width:100%">
                <tr style="background:#f0f0f0">
                  <td><b>Movie</b></td>
                  <td>{booking.Showtime?.Movie?.Title}</td>
                </tr>
                <tr>
                  <td><b>Hall</b></td>
                  <td>{booking.Showtime?.Hall?.Name}</td>
                </tr>
                <tr style="background:#f0f0f0">
                  <td><b>Showtime</b></td>
                  <td>{showtimeLocal} (Belgrade time)</td>
                </tr>
                <tr>
                  <td><b>Status</b></td>
                  <td><strong>{booking.Status}</strong></td>
                </tr>
              </table>
              <h3>Seats</h3>
              <table border="1" cellpadding="8" style="border-collapse:collapse;width:100%">
                <tr style="background:#1a1a2e;color:white">
                  <th>Seat</th><th>Type</th><th>Price</th>
                </tr>
                {string.Join("\n", seatRows)}
              </table>
              <p style="font-size:1.1em;margin-top:16px">
                <strong>Total: &euro;{booking.TotalPrice:F2}</strong>
              </p>
              <p style="color:#555;font-size:0.9em">Please arrive at least 15 minutes before the showtime. Enjoy the film!</p>
              <hr/>
              <p style="color:#888;font-size:0.85em">CinemaBooking &mdash; this is an automated message, please do not reply.</p>
            </div>
            """;

        await SendEmailAsync(
            user.Email!,
            user.GetFullName(),
            $"Booking Confirmation #{booking.Id} — {booking.Showtime?.Movie?.Title}",
            html,
            cancellationToken);
    }

    public async Task SendCancellationNoticeAsync(
        Booking booking,
        ApplicationUser user,
        CancellationToken cancellationToken = default)
    {
        var seatLabels = string.Join(", ",
            booking.BookingSeats.Select(bs => bs.GetSeatLabel()));

        var showtimeLocal = booking.Showtime?.StartTime is not null
            ? ToBelgrade(booking.Showtime.StartTime)
            : "N/A";

        var html = $"""
            <div style="font-family:Arial,sans-serif;max-width:600px;margin:0 auto">
              <h2 style="color:#c0392b">&#x274C; Booking Cancelled</h2>
              <p>Dear <strong>{user.GetFullName()}</strong>,</p>
              <p>Your booking has been cancelled.</p>
              <table border="1" cellpadding="8" style="border-collapse:collapse;width:100%">
                <tr style="background:#f0f0f0">
                  <td><b>Movie</b></td>
                  <td>{booking.Showtime?.Movie?.Title}</td>
                </tr>
                <tr>
                  <td><b>Hall</b></td>
                  <td>{booking.Showtime?.Hall?.Name}</td>
                </tr>
                <tr style="background:#f0f0f0">
                  <td><b>Showtime</b></td>
                  <td>{showtimeLocal} (Belgrade time)</td>
                </tr>
                <tr>
                  <td><b>Seats</b></td>
                  <td>{seatLabels}</td>
                </tr>
                <tr style="background:#f0f0f0">
                  <td><b>Booking Status</b></td>
                  <td><strong style="color:#c0392b">Cancelled</strong></td>
                </tr>
              </table>
              <p>We hope to see you again soon!</p>
              <hr/>
              <p style="color:#888;font-size:0.85em">CinemaBooking &mdash; this is an automated message, please do not reply.</p>
            </div>
            """;

        await SendEmailAsync(
            user.Email!,
            user.GetFullName(),
            $"Booking #{booking.Id} has been cancelled",
            html,
            cancellationToken);
    }

    public async Task SendPaymentConfirmationAsync(
        Payment payment,
        ApplicationUser user,
        CancellationToken cancellationToken = default)
    {
        var paymentDateLocal = ToBelgrade(payment.PaymentDate);

        var html = $"""
            <div style="font-family:Arial,sans-serif;max-width:600px;margin:0 auto">
              <h2 style="color:#27ae60">&#x1F4B3; Payment Confirmation</h2>
              <p>Dear <strong>{user.GetFullName()}</strong>,</p>
              <p>Your payment has been successfully processed.</p>
              <table border="1" cellpadding="8" style="border-collapse:collapse;width:100%">
                <tr style="background:#f0f0f0">
                  <td><b>Movie</b></td>
                  <td>{payment.Booking?.Showtime?.Movie?.Title}</td>
                </tr>
                <tr>
                  <td><b>Amount</b></td>
                  <td><strong>&euro;{payment.Amount:F2}</strong></td>
                </tr>
                <tr style="background:#f0f0f0">
                  <td><b>Payment Method</b></td>
                  <td>{payment.Method}</td>
                </tr>
                <tr>
                  <td><b>Payment Status</b></td>
                  <td><strong style="color:#27ae60">{payment.Status}</strong></td>
                </tr>
                <tr style="background:#f0f0f0">
                  <td><b>Payment Date</b></td>
                  <td>{paymentDateLocal} (Belgrade time)</td>
                </tr>
              </table>
              <hr/>
              <p style="color:#888;font-size:0.85em">CinemaBooking &mdash; this is an automated message, please do not reply.</p>
            </div>
            """;

        await SendEmailAsync(
            user.Email!,
            user.GetFullName(),
            $"Payment Confirmation #{payment.Id}",
            html,
            cancellationToken);
    }
    public async Task SendRefundConfirmationAsync(
        Payment payment,
        ApplicationUser user,
        CancellationToken cancellationToken = default)
    {
        var processedDateLocal = ToBelgrade(payment.PaymentDate);

        var html = $"""
            <div style="font-family:Arial,sans-serif;max-width:600px;margin:0 auto">
              <h2 style="color:#2980b9">&#x1F504; Refund Confirmation</h2>
              <p>Dear <strong>{user.GetFullName()}</strong>,</p>
              <p>Your refund for payment <strong>#{payment.Id}</strong> has been successfully processed.</p>
              <table border="1" cellpadding="8" style="border-collapse:collapse;width:100%">
                <tr style="background:#f0f0f0">
                  <td><b>Refund Amount</b></td>
                  <td><strong>&euro;{payment.Amount:F2}</strong></td>
                </tr>
                <tr>
                  <td><b>Status</b></td>
                  <td><strong style="color:#2980b9">Refunded</strong></td>
                </tr>
                <tr style="background:#f0f0f0">
                  <td><b>Processed Date</b></td>
                  <td>{processedDateLocal} (Belgrade time)</td>
                </tr>
              </table>
              <p>Funds should be returned within 3-5 business days.</p>
              <hr/>
              <p style="color:#888;font-size:0.85em">CinemaBooking &mdash; this is an automated message, please do not reply.</p>
            </div>
            """;

        await SendEmailAsync(
            user.Email!,
            user.GetFullName(),
            $"Refund Confirmation for payment #{payment.Id}",
            html,
            cancellationToken);
    }
}
