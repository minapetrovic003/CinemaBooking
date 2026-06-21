using CinemaBooking.Application.CQRS.Bookings.Commands;
using CinemaBooking.Application.Notifications;
using CinemaBooking.Application.Repositories;
using CinemaBooking.Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CinemaBooking.Application.CQRS.Bookings.Handlers;

public class CancelBookingHandler
    : IRequestHandler<CancelBookingCommand, (bool Success, string? ErrorMessage)>
{
    private readonly IUnitOfWork _uow;
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;
    private readonly ILogger<CancelBookingHandler> _logger;

    public CancelBookingHandler(
        IUnitOfWork uow,
        IUserRepository userRepository,
        INotificationService notificationService,
        ILogger<CancelBookingHandler> logger)
    {
        _uow = uow;
        _userRepository = userRepository;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<(bool Success, string? ErrorMessage)> Handle(
        CancelBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = _uow.Bookings.GetByIdWithDetails(request.Id);
        if (booking is null)
            return (false, (string?)null);

        // Ako je booking vec placen, ne moze se direktno otkazati - potreban je refund
        if (booking.Status == BookingStatus.Confirmed || booking.Status == BookingStatus.CheckedIn)
            return (false, "This booking has already been paid. To cancel, please request a payment refund from the admin.");

        if (!booking.Cancel())
            return (false, "Booking cannot be cancelled in its current status.");

        _uow.SaveChanges();

        try
        {
            var user = await _userRepository.FindByIdAsync(booking.UserId);
            if (user is not null)
                await _notificationService.SendCancellationNoticeAsync(booking, user, cancellationToken);
            else
                _logger.LogWarning(
                    "User not found for booking #{BookingId} - cancellation email not sent.",
                    booking.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Cancellation email failed for booking #{BookingId}.",
                booking.Id);
        }

        return (true, (string?)null);
    }
}