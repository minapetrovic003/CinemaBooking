using CinemaBooking.Application.CQRS.SeatLocks.Commands;
using CinemaBooking.Application.Repositories;
using MediatR;

namespace CinemaBooking.Application.CQRS.SeatLocks.Handlers;

public class ReleaseLocksHandler
    : IRequestHandler<ReleaseLocksCommand, (bool Success, string? ErrorMessage)>
{
    private readonly IUnitOfWork _uow;

    public ReleaseLocksHandler(IUnitOfWork uow) => _uow = uow;

    public Task<(bool Success, string? ErrorMessage)> Handle(
        ReleaseLocksCommand request, CancellationToken cancellationToken)
    {
        _uow.SeatLocks.ReleaseLocksForUser(request.ShowtimeId, request.UserId);
        _uow.SaveChanges();
        return Task.FromResult((true, (string?)null));
    }
}