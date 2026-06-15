using CinemaBooking.API.DTOs.Payments;
using CinemaBooking.Domain;
using CinemaBooking.Domain.Repositories;
using CinemaBooking.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace CinemaBooking.API.Services;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<ApplicationUser> _userManager;

    public PaymentService(IUnitOfWork uow, UserManager<ApplicationUser> userManager)
    {
        _uow = uow;
        _userManager = userManager;
    }

    public PaymentDto? GetById(long id)
    {
        var payment = _uow.Payments.GetByIdWithDetails(id);
        if (payment is null) return null;

        var user = payment.Booking is not null
            ? _userManager.Users.FirstOrDefault(u => u.Id == payment.Booking.UserId)
            : null;

        return MapToDto(payment, user);
    }

    public (PaymentDto? dto, string? errorMessage, int statusCode) Create(CreatePaymentRequest request)
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

        return (MapToDto(saved!, user), null, 201);
    }

    public (bool success, string? errorMessage) Refund(long id)
    {
        var payment = _uow.Payments.GetById(id);
        if (payment is null) return (false, null);

        if (!payment.Refund())
            return (false, "Payment cannot be refunded in its current status.");

        _uow.SaveChanges();
        return (true, null);
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