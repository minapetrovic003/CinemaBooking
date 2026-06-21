using CinemaBooking.Application.CQRS.Payments.Commands;
using CinemaBooking.Application.Notifications;
using CinemaBooking.Application.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CinemaBooking.Application.CQRS.Payments.Handlers;

public class RefundPaymentHandler : IRequestHandler<RefundPaymentCommand, (bool Success, string? ErrorMessage)>
{
    private readonly IUnitOfWork _uow;
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;
    private readonly ILogger<RefundPaymentHandler> _logger;

    public RefundPaymentHandler(
        IUnitOfWork uow,
        IUserRepository userRepository,
        INotificationService notificationService,
        ILogger<RefundPaymentHandler> logger)
    {
        _uow = uow;
        _userRepository = userRepository;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<(bool Success, string? ErrorMessage)> Handle(
        RefundPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = _uow.Payments.GetByIdWithDetails(request.Id);
        if (payment is null)
            return (false, null);

        if (!payment.Refund())
            return (false, "Payment cannot be refunded in its current status.");

        if (payment.Booking is not null)
            payment.Booking.CancelAfterRefund();

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
                    "User not found for payment #{PaymentId} - refund email not sent.", payment.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Refund email failed for payment #{PaymentId}.", payment.Id);
        }

        return (true, null);
    }
}