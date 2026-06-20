using CinemaBooking.Application.CQRS.Bookings.Commands;
using CinemaBooking.Application.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CinemaBooking.Application.CQRS.Bookings.Handlers;

public class CheckInBookingHandler
    : IRequestHandler<CheckInBookingCommand, (bool Success, string? ErrorMessage)>
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<CheckInBookingHandler> _logger;

    public CheckInBookingHandler(IUnitOfWork uow, ILogger<CheckInBookingHandler> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public Task<(bool Success, string? ErrorMessage)> Handle(
        CheckInBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = _uow.Bookings.GetById(request.Id);

        if (booking is null)
            return Task.FromResult((false, (string?)"Booking not found."));

        // Vlasnik može check-in svoju rezervaciju; Admin može check-in bilo koju
        if (!request.RequestingUserIsAdmin && booking.UserId != request.RequestingUserId)
            return Task.FromResult((false, (string?)"You can only check in your own booking."));

        if (!booking.CheckIn())
            return Task.FromResult((false, (string?)"Only confirmed (paid) bookings can be checked in."));

        _uow.SaveChanges();

        _logger.LogInformation(
            "Booking #{BookingId} checked in by user {UserId} (admin: {IsAdmin}).",
            booking.Id, request.RequestingUserId, request.RequestingUserIsAdmin);

        return Task.FromResult((true, (string?)null));
    }
}