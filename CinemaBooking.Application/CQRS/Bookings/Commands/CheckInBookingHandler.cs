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

    public async Task<(bool Success, string? ErrorMessage)> Handle(
        CheckInBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = _uow.Bookings.GetById(request.Id);

        if (booking is null)
            return (false, "Booking not found.");

        
        if (!booking.CheckIn())
            return (false, "Only confirmed (paid) bookings can be checked in.");

        await _uow.SaveChangesAsync();

        _logger.LogInformation(
            "Booking #{BookingId} checked in successfully.",
            booking.Id);

        return (true, null);
    }
}