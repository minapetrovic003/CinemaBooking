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

        // Check-in ruta je otvorena (AllowAnonymous) jer QR kod dobija
        // iskljucivo vlasnik rezervacije putem emaila
        if (!booking.CheckIn())
            return Task.FromResult((false, (string?)"Only confirmed (paid) bookings can be checked in."));

        _uow.SaveChanges();

        _logger.LogInformation(
            "Booking #{BookingId} checked in successfully.",
            booking.Id);

        return Task.FromResult((true, (string?)null));
    }
}