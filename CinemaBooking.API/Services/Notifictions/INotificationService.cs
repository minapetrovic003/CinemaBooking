using CinemaBooking.Domain;
using CinemaBooking.Infrastructure.Identity;

namespace CinemaBooking.API.Services.Notifications;

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

    Task SendBookingConfirmationAsync(
        Booking booking,
        ApplicationUser user,
        byte[] pdfTicket,
        CancellationToken cancellationToken = default);

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