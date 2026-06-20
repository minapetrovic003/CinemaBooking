using CinemaBooking.Application.Services.Notifications;
using CinemaBooking.Domain;
using CinemaBooking.Infrastructure.Identity;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

// NAPOMENA: namespace je ispravljen — bio je CinemaBooking.API.Services.Notifications
namespace CinemaBooking.Application.Notifications;

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

    // Konvertuje UTC DateTime u Belgrade prikaz
    private static string ToBelgrade(DateTime utcTime)
    {
        var local = TimeZoneInfo.ConvertTimeFromUtc(utcTime, BelgradeZone);
        return local.ToString("dd.MM.yyyy HH:mm");
    }

    // Generiše QR kod URL koji vodi na stranicu za verifikaciju rezervacije
    private string BuildQrImageUrl(long bookingId)
    {
        var verifyUrl = $"{_smtp.FrontendBaseUrl}/#verify/{bookingId}";
        var encoded = Uri.EscapeDataString(verifyUrl);
        return $"https://chart.googleapis.com/chart?cht=qr&chs=300x300&chl={encoded}&choe=UTF-8";
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
                $"<tr><td style=\"padding:8px;border:1px solid #ddd\">{bs.GetSeatLabel()}</td>" +
                $"<td style=\"padding:8px;border:1px solid #ddd\">{bs.Seat?.SeatType}</td>" +
                $"<td style=\"padding:8px;border:1px solid #ddd;text-align:right\">&euro;{bs.Price:F2}</td></tr>")
            .ToList();

        var showtimeLocal = booking.Showtime?.StartTime is not null
            ? ToBelgrade(booking.Showtime.StartTime)
            : "N/A";

        var qrImageUrl = BuildQrImageUrl(booking.Id);

        var html = $"""
            <div style="font-family:Arial,sans-serif;max-width:620px;margin:0 auto;color:#222">
              <div style="background:#1a0e0e;padding:24px;border-radius:8px 8px 0 0;text-align:center">
                <h1 style="color:#CC8B86;margin:0;font-size:1.8rem">CinemaVerse</h1>
                <p style="color:#b09a90;margin:6px 0 0">Booking Confirmation</p>
              </div>
              <div style="background:#fff;padding:28px;border:1px solid #eee;border-top:none">
                <h2 style="color:#1a0e0e;margin-top:0">&#x2705; Your booking is confirmed!</h2>
                <p>Dear <strong>{user.GetFullName()}</strong>,</p>
                <p>Thank you for your booking. Below are your details:</p>

                <table style="border-collapse:collapse;width:100%;margin-bottom:20px">
                  <tr style="background:#f7f0ed">
                    <td style="padding:10px;border:1px solid #ddd"><b>Movie</b></td>
                    <td style="padding:10px;border:1px solid #ddd">{booking.Showtime?.Movie?.Title}</td>
                  </tr>
                  <tr>
                    <td style="padding:10px;border:1px solid #ddd"><b>Hall</b></td>
                    <td style="padding:10px;border:1px solid #ddd">{booking.Showtime?.Hall?.Name}</td>
                  </tr>
                  <tr style="background:#f7f0ed">
                    <td style="padding:10px;border:1px solid #ddd"><b>Showtime</b></td>
                    <td style="padding:10px;border:1px solid #ddd">{showtimeLocal} (Belgrade time)</td>
                  </tr>
                  <tr>
                    <td style="padding:10px;border:1px solid #ddd"><b>Booking Status</b></td>
                    <td style="padding:10px;border:1px solid #ddd"><strong style="color:#27ae60">{booking.Status}</strong></td>
                  </tr>
                </table>

                <h3 style="color:#1a0e0e">Seat Details</h3>
                <table style="border-collapse:collapse;width:100%;margin-bottom:20px">
                  <tr style="background:#1a0e0e;color:#fff">
                    <th style="padding:10px;border:1px solid #444;text-align:left">Seat</th>
                    <th style="padding:10px;border:1px solid #444;text-align:left">Type</th>
                    <th style="padding:10px;border:1px solid #444;text-align:right">Price</th>
                  </tr>
                  {string.Join("\n", seatRows)}
                  <tr style="background:#f7f0ed;font-weight:bold">
                    <td colspan="2" style="padding:10px;border:1px solid #ddd">Total</td>
                    <td style="padding:10px;border:1px solid #ddd;text-align:right">&euro;{booking.TotalPrice:F2}</td>
                  </tr>
                </table>

                <div style="text-align:center;margin:28px 0">
                  <p style="color:#555;margin-bottom:12px"><b>Show this QR code at the cinema entrance</b></p>
                  <img src="{qrImageUrl}" alt="Booking QR Code" width="220" height="220"
                       style="border:4px solid #CC8B86;border-radius:8px;padding:8px" />
                  <p style="color:#888;font-size:0.8em;margin-top:8px">Booking #{booking.Id}</p>
                </div>

                <p style="color:#555;font-size:0.9em">Please arrive at least 15 minutes before the showtime.</p>
                <hr style="border:none;border-top:1px solid #eee;margin:20px 0"/>
                <p style="color:#aaa;font-size:0.8em;text-align:center">
                  CinemaVerse &mdash; this is an automated message, please do not reply.
                </p>
              </div>
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
            <div style="font-family:Arial,sans-serif;max-width:620px;margin:0 auto;color:#222">
              <div style="background:#1a0e0e;padding:24px;border-radius:8px 8px 0 0;text-align:center">
                <h1 style="color:#CC8B86;margin:0;font-size:1.8rem">CinemaVerse</h1>
                <p style="color:#b09a90;margin:6px 0 0">Booking Cancellation</p>
              </div>
              <div style="background:#fff;padding:28px;border:1px solid #eee;border-top:none">
                <h2 style="color:#c0392b;margin-top:0">&#x274C; Your booking has been cancelled</h2>
                <p>Dear <strong>{user.GetFullName()}</strong>,</p>
                <p>The following booking has been successfully cancelled:</p>
                <table style="border-collapse:collapse;width:100%;margin-bottom:20px">
                  <tr style="background:#f7f0ed">
                    <td style="padding:10px;border:1px solid #ddd"><b>Movie</b></td>
                    <td style="padding:10px;border:1px solid #ddd">{booking.Showtime?.Movie?.Title}</td>
                  </tr>
                  <tr>
                    <td style="padding:10px;border:1px solid #ddd"><b>Hall</b></td>
                    <td style="padding:10px;border:1px solid #ddd">{booking.Showtime?.Hall?.Name}</td>
                  </tr>
                  <tr style="background:#f7f0ed">
                    <td style="padding:10px;border:1px solid #ddd"><b>Showtime</b></td>
                    <td style="padding:10px;border:1px solid #ddd">{showtimeLocal} (Belgrade time)</td>
                  </tr>
                  <tr>
                    <td style="padding:10px;border:1px solid #ddd"><b>Seats</b></td>
                    <td style="padding:10px;border:1px solid #ddd">{seatLabels}</td>
                  </tr>
                  <tr style="background:#f7f0ed">
                    <td style="padding:10px;border:1px solid #ddd"><b>Status</b></td>
                    <td style="padding:10px;border:1px solid #ddd"><strong style="color:#c0392b">Cancelled</strong></td>
                  </tr>
                </table>
                <p>If you did not request this cancellation, please contact us immediately.</p>
                <p>We hope to see you again at CinemaVerse!</p>
                <hr style="border:none;border-top:1px solid #eee;margin:20px 0"/>
                <p style="color:#aaa;font-size:0.8em;text-align:center">
                  CinemaVerse &mdash; this is an automated message, please do not reply.
                </p>
              </div>
            </div>
            """;

        await SendEmailAsync(
            user.Email!,
            user.GetFullName(),
            $"Booking #{booking.Id} Cancelled — {booking.Showtime?.Movie?.Title}",
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
            <div style="font-family:Arial,sans-serif;max-width:620px;margin:0 auto;color:#222">
              <div style="background:#1a0e0e;padding:24px;border-radius:8px 8px 0 0;text-align:center">
                <h1 style="color:#CC8B86;margin:0;font-size:1.8rem">CinemaVerse</h1>
                <p style="color:#b09a90;margin:6px 0 0">Payment Confirmation</p>
              </div>
              <div style="background:#fff;padding:28px;border:1px solid #eee;border-top:none">
                <h2 style="color:#27ae60;margin-top:0">&#x1F4B3; Payment Successful</h2>
                <p>Dear <strong>{user.GetFullName()}</strong>,</p>
                <p>Your payment has been successfully processed.</p>
                <table style="border-collapse:collapse;width:100%;margin-bottom:20px">
                  <tr style="background:#f7f0ed">
                    <td style="padding:10px;border:1px solid #ddd"><b>Movie</b></td>
                    <td style="padding:10px;border:1px solid #ddd">{payment.Booking?.Showtime?.Movie?.Title}</td>
                  </tr>
                  <tr>
                    <td style="padding:10px;border:1px solid #ddd"><b>Amount Paid</b></td>
                    <td style="padding:10px;border:1px solid #ddd"><strong>&euro;{payment.Amount:F2}</strong></td>
                  </tr>
                  <tr style="background:#f7f0ed">
                    <td style="padding:10px;border:1px solid #ddd"><b>Payment Method</b></td>
                    <td style="padding:10px;border:1px solid #ddd">{payment.Method}</td>
                  </tr>
                  <tr>
                    <td style="padding:10px;border:1px solid #ddd"><b>Status</b></td>
                    <td style="padding:10px;border:1px solid #ddd"><strong style="color:#27ae60">{payment.Status}</strong></td>
                  </tr>
                  <tr style="background:#f7f0ed">
                    <td style="padding:10px;border:1px solid #ddd"><b>Payment Date</b></td>
                    <td style="padding:10px;border:1px solid #ddd">{paymentDateLocal} (Belgrade time)</td>
                  </tr>
                </table>
                <hr style="border:none;border-top:1px solid #eee;margin:20px 0"/>
                <p style="color:#aaa;font-size:0.8em;text-align:center">
                  CinemaVerse &mdash; this is an automated message, please do not reply.
                </p>
              </div>
            </div>
            """;

        await SendEmailAsync(
            user.Email!,
            user.GetFullName(),
            $"Payment Confirmation #{payment.Id} — CinemaVerse",
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
            <div style="font-family:Arial,sans-serif;max-width:620px;margin:0 auto;color:#222">
              <div style="background:#1a0e0e;padding:24px;border-radius:8px 8px 0 0;text-align:center">
                <h1 style="color:#CC8B86;margin:0;font-size:1.8rem">CinemaVerse</h1>
                <p style="color:#b09a90;margin:6px 0 0">Refund Confirmation</p>
              </div>
              <div style="background:#fff;padding:28px;border:1px solid #eee;border-top:none">
                <h2 style="color:#2980b9;margin-top:0">&#x1F504; Refund Processed</h2>
                <p>Dear <strong>{user.GetFullName()}</strong>,</p>
                <p>Your refund for payment <strong>#{payment.Id}</strong> has been successfully processed.</p>
                <table style="border-collapse:collapse;width:100%;margin-bottom:20px">
                  <tr style="background:#f7f0ed">
                    <td style="padding:10px;border:1px solid #ddd"><b>Refund Amount</b></td>
                    <td style="padding:10px;border:1px solid #ddd"><strong>&euro;{payment.Amount:F2}</strong></td>
                  </tr>
                  <tr>
                    <td style="padding:10px;border:1px solid #ddd"><b>Status</b></td>
                    <td style="padding:10px;border:1px solid #ddd"><strong style="color:#2980b9">Refunded</strong></td>
                  </tr>
                  <tr style="background:#f7f0ed">
                    <td style="padding:10px;border:1px solid #ddd"><b>Processed Date</b></td>
                    <td style="padding:10px;border:1px solid #ddd">{processedDateLocal} (Belgrade time)</td>
                  </tr>
                </table>
                <p>Funds should be returned to your account within 3–5 business days.</p>
                <hr style="border:none;border-top:1px solid #eee;margin:20px 0"/>
                <p style="color:#aaa;font-size:0.8em;text-align:center">
                  CinemaVerse &mdash; this is an automated message, please do not reply.
                </p>
              </div>
            </div>
            """;

        await SendEmailAsync(
            user.Email!,
            user.GetFullName(),
            $"Refund Confirmation — Payment #{payment.Id}",
            html,
            cancellationToken);
    }
}