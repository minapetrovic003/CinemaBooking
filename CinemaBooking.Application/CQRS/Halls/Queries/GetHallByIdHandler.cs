using CinemaBooking.Application.CQRS.Halls.Queries;
using CinemaBooking.Application.Repositories;
using CinemaBooking.Domain.DTOs.Halls;
using MediatR;

namespace CinemaBooking.Application.CQRS.Halls.Handlers;

public class GetHallByIdHandler : IRequestHandler<GetHallByIdQuery, HallDto?>
{
    private readonly IUnitOfWork _uow;

    public GetHallByIdHandler(IUnitOfWork uow) => _uow = uow;

    public Task<HallDto?> Handle(GetHallByIdQuery request, CancellationToken cancellationToken)
    {
        var hall = _uow.Halls.GetByIdWithSeats(request.Id);

        if (hall is null)
            return Task.FromResult<HallDto?>(null);

        var dto = new HallDto
        {
            Id = hall.Id,
            Name = hall.Name,
            Capacity = hall.Capacity,
            SeatCount = hall.Seats.Count,
            Seats = hall.Seats
                .OrderBy(s => s.Row)
                .ThenBy(s => s.Number)
                .Select(s => new SeatInfo
                {
                    Id = s.Id,
                    Label = s.GetSeatLabel(),
                    Row = s.Row,
                    Number = s.Number,
                    SeatType = s.SeatType.ToString()
                }).ToList()
        };

        return Task.FromResult<HallDto?>(dto);
    }
}