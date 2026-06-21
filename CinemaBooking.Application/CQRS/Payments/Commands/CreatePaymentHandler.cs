using CinemaBooking.Application.CQRS.Payments.Commands;
using CinemaBooking.Application.CQRS.Payments.Handlers;
using CinemaBooking.Application.Notifications;
using CinemaBooking.Application.Repositories;
using CinemaBooking.Domain.DTOs.Payments;
using CinemaBooking.Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CinemaBooking.Application.CQRS.Payments.Handlers;

public class CreatePaymentHandler : IRequestHandler<CreatePaymentCommand, PaymentDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;
    private readonly IPdfTicketService _pdfTicketService;
    private readonly ILogger<CreatePaymentHandler> _logger;

    public CreatePaymentHandler(
        IUnitOfWork uow,
        IUserRepository userRepository,
        INotificationService notificationService,
        IPdfTicketService pdfTicketService,
        ILogger<CreatePaymentHandler> logger)
    {
        _uow = uow;
        _userRepository = userRepository;
        _notificationService = notificationService;
        _pdfTicketService = pdfTicketService;
        _logger = logger;
    }

    public async Task<PaymentDto> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<PaymentMethod>(request.Method, true, out var method))
            throw new ArgumentException(
                $"Invalid payment method '{request.Method}'. Valid: CreditCard, DebitCard, PayPal, Voucher.");

        var booking = _uow.Bookings
            .Search(request.UserEmail, null, null, null)
            .FirstOrDefault(b =>
                b.Showtime?.Movie?.Title == request.MovieTitle &&
                b.Showtime?.Hall?.Name == request.HallName &&
                b.Showtime?.StartTime == request.ShowtimeStartTime &&
                b.Status == BookingStatus.Pending)
            ?? throw new KeyNotFoundException(
                "Pending booking not found. Check user email, movie title, hall name, and showtime.");

        var bookingWithPayment = _uow.Bookings.GetByIdWithDetails(booking.Id);
        if (bookingWithPayment?.Payment is not null)
            throw new InvalidOperationException("This booking already has a payment.");

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
                await _notificationService.SendBookingConfirmationAsync(confirmedBooking, user, pdfTicket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "PDF/email failed for booking #{BookingId} after payment #{PaymentId}.",
                    booking.Id, saved.Id);
            }
        }

        return GetAllPaymentsHandler.MapToDto(saved!, user?.FullName, user?.Email);
    }
}