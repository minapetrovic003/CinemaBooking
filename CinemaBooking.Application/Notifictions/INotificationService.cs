using CinemaBooking.Domain;
using CinemaBooking.Infrastructure.Identity;

namespace CinemaBooking.Application.Notifications;

public interface INotificationService
{
    Task SendEmailAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        byte[]? attachmentBytes = null,
        string? attachmentFileName = null,
        CancellationToken cancellationToken = default);

    // Rezervacija — šalje email + PDF karta kao prilog
    Task SendBookingConfirmationAsync(
        Booking booking,
        ApplicationUser user,
        byte[] pdfTicket,
        CancellationToken cancellationToken = default);

    // Otkazivanje — samo email, bez PDF-a
    Task SendCancellationNoticeAsync(
        Booking booking,
        ApplicationUser user,
        CancellationToken cancellationToken = default);

    Task SendPaymentConfirmationAsync(
        Payment payment,
        ApplicationUser user,
        CancellationToken cancellationToken = default);

    Task SendRefundConfirmationAsync(
        Payment payment,
        ApplicationUser user,
        CancellationToken cancellationToken = default);
}