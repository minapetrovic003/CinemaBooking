using CinemaBooking.API.CQRS.Bookings.Commands;
using CinemaBooking.Domain.Repositories;
using MediatR;

namespace CinemaBooking.API.CQRS.Bookings.Handlers;

public class CancelBookingHandler
    : IRequestHandler<CancelBookingCommand, (bool Success, string? ErrorMessage)>
{
    private readonly IUnitOfWork _uow;

    public CancelBookingHandler(IUnitOfWork uow) => _uow = uow;

    public Task<(bool Success, string? ErrorMessage)> Handle(
        CancelBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = _uow.Bookings.GetById(request.Id);
        if (booking is null)
            return Task.FromResult((false, (string?)null));

        if (!booking.Cancel())
            return Task.FromResult((false, (string?)"Booking cannot be canceled in its current status."));

        _uow.SaveChanges();
        return Task.FromResult((true, (string?)null));
    }
}