using CinemaBooking.Application.CQRS.Halls.Commands;
using CinemaBooking.Application.Repositories;
using CinemaBooking.Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CinemaBooking.Application.CQRS.Halls.Handlers;

public class UpdateSeatTypeHandler
    : IRequestHandler<UpdateSeatTypeCommand, (bool Success, string? ErrorMessage, int StatusCode)>
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<UpdateSeatTypeHandler> _logger;

    public UpdateSeatTypeHandler(IUnitOfWork uow, ILogger<UpdateSeatTypeHandler> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<(bool Success, string? ErrorMessage, int StatusCode)> Handle(
        UpdateSeatTypeCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<SeatType>(request.SeatType, ignoreCase: true, out var seatType))
            return (false,
                $"Invalid seat type '{request.SeatType}'. Valid values: Standard, Vip, Wheelchair.",
                400);

        var seat = _uow.Seats.GetById(request.SeatId);

        if (seat is null || seat.HallId != request.HallId)
            return (false,
                $"Seat {request.SeatId} not found in hall {request.HallId}.",
                404);

        seat.SeatType = seatType;
        await _uow.SaveChangesAsync();

        _logger.LogInformation(
            "Seat {SeatId} (Hall {HallId}) type changed to {SeatType}.",
            seat.Id, request.HallId, seatType);

        return (true, null, 204);
    }
}