using CinemaBooking.Domain;
using CinemaBooking.Infrastructure.Identity;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using System.Net.Mail;

namespace CinemaBooking.API.Services.Notifications;

public class NotificationService : INotificationService
{
    private readonly SmtpSettings _smtp;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IOptions<SmtpSettings> smtpOptions,
        ILogger<NotificationService> logger)
    {
        _smtp = smtpOptions.Value;
        _logger = logger;
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
                $"<tr><td>{bs.GetSeatLabel()}</td><td>{bs.Seat?.SeatType}</td><td>{bs.Price:C}</td></tr>")
            .ToList();

        var html = $"""
            <div style="font-family:Arial,sans-serif;max-width:600px;margin:0 auto">
              <h2 style="color:#1a1a2e">✅ Potvrda rezervacije</h2>
              <p>Poštovani/a <strong>{user.GetFullName()}</strong>,</p>
              <p>Vaša rezervacija je uspješno kreirana.</p>
              <table border="1" cellpadding="8" style="border-collapse:collapse;width:100%">
                <tr style="background:#f0f0f0">
                  <td><b>Film</b></td>
                  <td>{booking.Showtime?.Movie?.Title}</td>
                </tr>
                <tr>
                  <td><b>Sala</b></td>
                  <td>{booking.Showtime?.Hall?.Name}</td>
                </tr>
                <tr style="background:#f0f0f0">
                  <td><b>Početak projekcije</b></td>
                  <td>{booking.Showtime?.StartTime:dd.MM.yyyy HH:mm}</td>
                </tr>
                <tr>
                  <td><b>Status</b></td>
                  <td><strong>{booking.Status}</strong></td>
                </tr>
              </table>
              <h3>Sjedišta</h3>
              <table border="1" cellpadding="8" style="border-collapse:collapse;width:100%">
                <tr style="background:#1a1a2e;color:white">
                  <th>Oznaka</th><th>Tip</th><th>Cijena</th>
                </tr>
                {string.Join("\n", seatRows)}
              </table>
              <p style="font-size:1.1em;margin-top:16px">
                <strong>Ukupna cijena: {booking.TotalPrice:C}</strong>
              </p>
              <hr/>
              <p style="color:#888;font-size:0.85em">CinemaBooking — automatska poruka</p>
            </div>
            """;

        await SendEmailAsync(
            user.Email!,
            user.GetFullName(),
            $"Potvrda rezervacije #{booking.Id} — {booking.Showtime?.Movie?.Title}",
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

        var html = $"""
            <div style="font-family:Arial,sans-serif;max-width:600px;margin:0 auto">
              <h2 style="color:#c0392b">❌ Rezervacija otkazana</h2>
              <p>Poštovani/a <strong>{user.GetFullName()}</strong>,</p>
              <p>Vaša rezervacija je otkazana.</p>
              <table border="1" cellpadding="8" style="border-collapse:collapse;width:100%">
                <tr style="background:#f0f0f0">
                  <td><b>Film</b></td>
                  <td>{booking.Showtime?.Movie?.Title}</td>
                </tr>
                <tr>
                  <td><b>Sala</b></td>
                  <td>{booking.Showtime?.Hall?.Name}</td>
                </tr>
                <tr style="background:#f0f0f0">
                  <td><b>Termin</b></td>
                  <td>{booking.Showtime?.StartTime:dd.MM.yyyy HH:mm}</td>
                </tr>
                <tr>
                  <td><b>Sjedišta</b></td>
                  <td>{seatLabels}</td>
                </tr>
                <tr style="background:#f0f0f0">
                  <td><b>Status rezervacije</b></td>
                  <td><strong style="color:#c0392b">Otkazana</strong></td>
                </tr>
              </table>
              <p>Nadamo se da ćete nas posjetiti ponovo!</p>
              <hr/>
              <p style="color:#888;font-size:0.85em">CinemaBooking — automatska poruka</p>
            </div>
            """;

        await SendEmailAsync(
            user.Email!,
            user.GetFullName(),
            $"Rezervacija #{booking.Id} je otkazana",
            html,
            cancellationToken);
    }

    public async Task SendPaymentConfirmationAsync(
        Payment payment,
        ApplicationUser user,
        CancellationToken cancellationToken = default)
    {
        var html = $"""
            <div style="font-family:Arial,sans-serif;max-width:600px;margin:0 auto">
              <h2 style="color:#27ae60">💳 Potvrda plaćanja</h2>
              <p>Poštovani/a <strong>{user.GetFullName()}</strong>,</p>
              <p>Vaše plaćanje je uspješno obrađeno.</p>
              <table border="1" cellpadding="8" style="border-collapse:collapse;width:100%">
                <tr style="background:#f0f0f0">
                  <td><b>Film</b></td>
                  <td>{payment.Booking?.Showtime?.Movie?.Title}</td>
                </tr>
                <tr>
                  <td><b>Iznos</b></td>
                  <td><strong>{payment.Amount:C}</strong></td>
                </tr>
                <tr style="background:#f0f0f0">
                  <td><b>Metoda plaćanja</b></td>
                  <td>{payment.Method}</td>
                </tr>
                <tr>
                  <td><b>Status plaćanja</b></td>
                  <td><strong style="color:#27ae60">{payment.Status}</strong></td>
                </tr>
                <tr style="background:#f0f0f0">
                  <td><b>Datum plaćanja</b></td>
                  <td>{payment.PaymentDate:dd.MM.yyyy HH:mm}</td>
                </tr>
              </table>
              <hr/>
              <p style="color:#888;font-size:0.85em">CinemaBooking — automatska poruka</p>
            </div>
            """;

        await SendEmailAsync(
            user.Email!,
            user.GetFullName(),
            $"Potvrda plaćanja #{payment.Id}",
            html,
            cancellationToken);
    }

    public async Task SendRefundConfirmationAsync(
        Payment payment,
        ApplicationUser user,
        CancellationToken cancellationToken = default)
    {
        var html = $"""
            <div style="font-family:Arial,sans-serif;max-width:600px;margin:0 auto">
              <h2 style="color:#2980b9">🔄 Potvrda povrata novca</h2>
              <p>Poštovani/a <strong>{user.GetFullName()}</strong>,</p>
              <p>Povrat novca za plaćanje <strong>#{payment.Id}</strong> je uspješno obrađen.</p>
              <table border="1" cellpadding="8" style="border-collapse:collapse;width:100%">
                <tr style="background:#f0f0f0">
                  <td><b>Iznos povrata</b></td>
                  <td><strong>{payment.Amount:C}</strong></td>
                </tr>
                <tr>
                  <td><b>Status</b></td>
                  <td><strong style="color:#2980b9">Refunded</strong></td>
                </tr>
                <tr style="background:#f0f0f0">
                  <td><b>Datum obrade</b></td>
                  <td>{payment.PaymentDate:dd.MM.yyyy HH:mm}</td>
                </tr>
              </table>
              <p>Sredstva bi trebala biti vraćena u roku 3-5 radnih dana.</p>
              <hr/>
              <p style="color:#888;font-size:0.85em">CinemaBooking — automatska poruka</p>
            </div>
            """;

        await SendEmailAsync(
            user.Email!,
            user.GetFullName(),
            $"Povrat novca za plaćanje #{payment.Id}",
            html,
            cancellationToken);
    }
}