using CinemaBooking.Application.Notifications;
using CinemaBooking.Application.Repositories;
using CinemaBooking.Domain.DTOs.Payments;
using CinemaBooking.Domain.Models;
using Microsoft.Extensions.Logging;

namespace CinemaBooking.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _uow;
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;
    private readonly IPdfTicketService _pdfTicketService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IUnitOfWork uow,
        IUserRepository userRepository,
        INotificationService notificationService,
        IPdfTicketService pdfTicketService,
        ILogger<PaymentService> logger)
    {
        _uow = uow;
        _userRepository = userRepository;
        _notificationService = notificationService;
        _pdfTicketService = pdfTicketService;
        _logger = logger;
    }

    public async Task<PaymentDto?> GetByIdAsync(long id)
    {
        var payment = _uow.Payments.GetByIdWithDetails(id);
        if (payment is null) return null;

        var user = payment.Booking is not null
            ? await _userRepository.FindByIdAsync(payment.Booking.UserId)
            : null;

        return MapToDto(payment, user?.FullName, user?.Email);
    }

    public async Task<(PaymentDto? dto, string? errorMessage, int statusCode)> CreateAsync(
        CreatePaymentRequest request)
    {
        if (!Enum.TryParse<PaymentMethod>(request.Method, true, out var method))
            return (null,
                $"Invalid payment method '{request.Method}'. Valid: CreditCard, DebitCard, PayPal, Voucher.",
                400);

        var booking = _uow.Bookings
            .Search(request.UserEmail, null, null, null)
            .FirstOrDefault(b =>
                b.Showtime?.Movie?.Title == request.MovieTitle &&
                b.Showtime?.Hall?.Name == request.HallName &&
                b.Showtime?.StartTime == request.ShowtimeStartTime &&
                b.Status == BookingStatus.Pending);

        if (booking is null)
            return (null,
                "Pending booking not found. Check user email, movie title, hall name, and showtime.",
                404);

        var bookingWithPayment = _uow.Bookings.GetByIdWithDetails(booking.Id);
        if (bookingWithPayment?.Payment is not null)
            return (null, "This booking already has a payment.", 409);

        var payment = new Payment
        {
            BookingId = booking.Id,
            Amount = booking.TotalPrice,
            Method = method,
            Status = PaymentStatus.Pending,
            PaymentDate = DateTime.UtcNow
        };

        payment.ProcessPayment();
        _uow.Payments.Add(payment);

        booking.Confirm();
        _uow.SaveChanges();

        var saved = _uow.Payments.GetByIdWithDetails(payment.Id);
        var confirmedBooking = _uow.Bookings.GetByIdWithDetails(booking.Id);
        var user = await _userRepository.FindByIdAsync(booking.UserId);

        if (saved is not null && confirmedBooking is not null && user is not null)
        {
            try
            {
                var pdfTicket = _pdfTicketService.GenerateTicket(confirmedBooking, user);
                await _notificationService.SendBookingConfirmationAsync(
                    confirmedBooking, user, pdfTicket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "PDF/email failed for booking #{BookingId} after payment #{PaymentId}.",
                    booking.Id, saved.Id);
            }
        }

        return (MapToDto(saved!, user?.FullName, user?.Email), null, 201);
    }

    public async Task<(bool success, string? errorMessage)> RefundAsync(long id)
    {
        var payment = _uow.Payments.GetByIdWithDetails(id);
        if (payment is null) return (false, null);

        if (!payment.Refund())
            return (false, "Payment cannot be refunded in its current status.");

        // Nakon refunda, otkazati booking (Confirmed/CheckedIn -> Canceled)
        if (payment.Booking is not null)
        {
            payment.Booking.CancelAfterRefund();
        }

        _uow.SaveChanges();

        try
        {
            var user = payment.Booking is not null
                ? await _userRepository.FindByIdAsync(payment.Booking.UserId)
                : null;

            if (user is not null)
                await _notificationService.SendRefundConfirmationAsync(payment, user);
            else
                _logger.LogWarning(
                    "User not found for payment #{PaymentId} - refund email not sent.",
                    payment.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Refund email failed for payment #{PaymentId}.",
                payment.Id);
        }

        return (true, null);
    }

    public IEnumerable<PaymentDto> GetAll()
    {
        var payments = _uow.Payments.GetAllWithDetails();
        return payments.Select(p =>
        {
            var user = p.Booking is not null
                ? _userRepository.FindByIdAsync(p.Booking.UserId).GetAwaiter().GetResult()
                : null;
            return MapToDto(p, user?.FullName, user?.Email);
        });
    }

    private static PaymentDto MapToDto(Payment p, string? fullName, string? email) => new()
    {
        Id = p.Id,
        Amount = p.Amount,
        PaymentDate = p.PaymentDate,
        Status = p.Status.ToString(),
        Method = p.Method.ToString(),
        UserFullName = fullName ?? string.Empty,
        UserEmail = email ?? string.Empty,
        MovieTitle = p.Booking?.Showtime?.Movie?.Title ?? string.Empty,
        ShowtimeStart = p.Booking?.Showtime?.StartTime ?? DateTime.MinValue,
        BookingStatus = p.Booking?.Status.ToString() ?? string.Empty
    };
}