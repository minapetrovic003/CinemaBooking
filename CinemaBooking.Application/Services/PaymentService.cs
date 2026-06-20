using CinemaBooking.Domain.DTOs.Payments;
using CinemaBooking.Application.Notifications;
using CinemaBooking.Domain.Repositories;
using CinemaBooking.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using CinemaBooking.Domain.Models;

namespace CinemaBooking.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly INotificationService _notificationService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IUnitOfWork uow,
        UserManager<ApplicationUser> userManager,
        INotificationService notificationService,
        ILogger<PaymentService> logger)
    {
        _uow = uow;
        _userManager = userManager;
        _notificationService = notificationService;
        _logger = logger;
    }

    public Task<PaymentDto?> GetByIdAsync(long id)
    {
        var payment = _uow.Payments.GetByIdWithDetails(id);
        if (payment is null) return Task.FromResult<PaymentDto?>(null);

        var user = payment.Booking is not null
            ? _userManager.Users.FirstOrDefault(u => u.Id == payment.Booking.UserId)
            : null;

        return Task.FromResult<PaymentDto?>(MapToDto(payment, user));
    }

    public async Task<(PaymentDto? dto, string? errorMessage, int statusCode)> CreateAsync(CreatePaymentRequest request)
    {
        if (!Enum.TryParse<PaymentMethod>(request.Method, true, out var method))
            return (null, $"Invalid payment method '{request.Method}'. Valid: CreditCard, DebitCard, PayPal, Voucher.", 400);

        var booking = _uow.Bookings
            .Search(request.UserEmail, null, null, null)
            .FirstOrDefault(b =>
                b.Showtime?.Movie?.Title == request.MovieTitle &&
                b.Showtime?.Hall?.Name == request.HallName &&
                b.Showtime?.StartTime == request.ShowtimeStartTime &&
                b.Status != BookingStatus.Canceled);

        if (booking is null)
            return (null, "Booking not found. Check user email, movie title, hall name, and showtime.", 404);

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
        _uow.SaveChanges();

        var saved = _uow.Payments.GetByIdWithDetails(payment.Id);
        var user = saved?.Booking is not null
            ? _userManager.Users.FirstOrDefault(u => u.Id == saved.Booking.UserId)
            : null;

        if (saved is not null && user is not null)
        {
            try
            {
                await _notificationService.SendPaymentConfirmationAsync(saved, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Payment confirmation email failed for payment #{PaymentId}, user {Email}.",
                    saved.Id, user.Email);
            }
        }

        return (MapToDto(saved!, user), null, 201);
    }

    public async Task<(bool success, string? errorMessage)> RefundAsync(long id)
    {
        var payment = _uow.Payments.GetByIdWithDetails(id);
        if (payment is null) return (false, null);

        if (!payment.Refund())
            return (false, "Payment cannot be refunded in its current status.");

        _uow.SaveChanges();

        try
        {
            var user = payment.Booking is not null
                ? await _userManager.FindByIdAsync(payment.Booking.UserId)
                : null;

            if (user is not null)
                await _notificationService.SendRefundConfirmationAsync(payment, user);
            else
                _logger.LogWarning(
                    "User not found for payment #{PaymentId} — refund email not sent.",
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
                ? _userManager.Users.FirstOrDefault(u => u.Id == p.Booking.UserId)
                : null;
            return MapToDto(p, user);
        });
    }

    private static PaymentDto MapToDto(Payment p, ApplicationUser? user) => new()
    {
        Id = p.Id,
        Amount = p.Amount,
        PaymentDate = p.PaymentDate,
        Status = p.Status.ToString(),
        Method = p.Method.ToString(),
        UserFullName = user?.GetFullName() ?? string.Empty,
        UserEmail = user?.Email ?? string.Empty,
        MovieTitle = p.Booking?.Showtime?.Movie?.Title ?? string.Empty,
        ShowtimeStart = p.Booking?.Showtime?.StartTime ?? DateTime.MinValue,
        BookingStatus = p.Booking?.Status.ToString() ?? string.Empty
    };
}