using CinemaBooking.Application.CQRS.Showtimes.Commands;
using CinemaBooking.Application.Repositories;
using MediatR;

namespace CinemaBooking.Application.CQRS.Showtimes.Handlers;

public class DeleteShowtimeHandler : IRequestHandler<DeleteShowtimeCommand, bool>
{
    private readonly IUnitOfWork _uow;

    public DeleteShowtimeHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<bool> Handle(DeleteShowtimeCommand request, CancellationToken cancellationToken)
    {
        var showtime = _uow.Showtimes.GetById(request.Id);
        if (showtime is null) return false;

        _uow.Showtimes.Remove(showtime);
        await _uow.SaveChangesAsync();
        return true;
    }
}