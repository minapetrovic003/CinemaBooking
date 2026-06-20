using CinemaBooking.Domain.DTOs.Users;
using CinemaBooking.Domain.Models;

namespace CinemaBooking.Application.Notifications;

public interface INotificationService
{
    Task SendEmailAsync(
        string toEmail, string toName, string subject, string htmlBody,
        byte[]? attachmentBytes = null, string? attachmentFileName = null,
        CancellationToken cancellationToken = default);

    Task SendBookingConfirmationAsync(
        Booking booking, UserInfo user, byte[] pdfTicket,
        CancellationToken cancellationToken = default);

    Task SendCancellationNoticeAsync(
        Booking booking, UserInfo user,
        CancellationToken cancellationToken = default);

    Task SendPaymentConfirmationAsync(
        Payment payment, UserInfo user,
        CancellationToken cancellationToken = default);

    Task SendRefundConfirmationAsync(
        Payment payment, UserInfo user,
        CancellationToken cancellationToken = default);
}