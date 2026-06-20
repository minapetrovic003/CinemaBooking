using CinemaBooking.Application.CQRS.Bookings.Commands;
using CinemaBooking.Application.Notifications;
using CinemaBooking.Domain.Repositories;
using CinemaBooking.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace CinemaBooking.Application.CQRS.Bookings.Handlers;

public class CancelBookingHandler
    : IRequestHandler<CancelBookingCommand, (bool Success, string? ErrorMessage)>
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly INotificationService _notificationService;
    private readonly ILogger<CancelBookingHandler> _logger;

    public CancelBookingHandler(
        IUnitOfWork uow,
        UserManager<ApplicationUser> userManager,
        INotificationService notificationService,
        ILogger<CancelBookingHandler> logger)
    {
        _uow = uow;
        _userManager = userManager;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<(bool Success, string? ErrorMessage)> Handle(
        CancelBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = _uow.Bookings.GetByIdWithDetails(request.Id);
        if (booking is null)
            return (false, (string?)null);

        if (!booking.Cancel())
            return (false, "Booking cannot be cancelled in its current status.");

        _uow.SaveChanges();

        try
        {
            var user = await _userManager.FindByIdAsync(booking.UserId);
            if (user is not null)
                await _notificationService.SendCancellationNoticeAsync(booking, user, cancellationToken);
            else
                _logger.LogWarning(
                    "User not found for booking #{BookingId} — cancellation email not sent.",
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