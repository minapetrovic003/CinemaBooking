using CinemaBooking.Domain.DTOs.Halls;
using CinemaBooking.Domain.Models;
using CinemaBooking.Application.Repositories;

namespace CinemaBooking.Application.Services;

public class HallService : IHallService
{
    private readonly IUnitOfWork _uow;

    public HallService(IUnitOfWork uow) => _uow = uow;

    public IEnumerable<HallDto> GetAll() =>
        _uow.Halls.GetAll().Select(h => new HallDto
        {
            Id = h.Id,
            Name = h.Name,
            Capacity = h.Capacity,
            SeatCount = h.Seats?.Count ?? 0
        });

    public HallDto? GetById(long id)
    {
        var hall = _uow.Halls.GetByIdWithSeats(id);
        if (hall is null) return null;

        return new HallDto
        {
            Id = hall.Id,
            Name = hall.Name,
            Capacity = hall.Capacity,
            SeatCount = hall.Seats.Count,
            Seats = hall.Seats.Select(s => new SeatInfo
            {
                Label = s.GetSeatLabel(),
                Row = s.Row,
                Number = s.Number,
                SeatType = s.SeatType.ToString()
            }).ToList()
        };
    }

    public (HallDto? dto, string? conflictMessage) Create(CreateHallRequest request)
    {
        if (_uow.Halls.GetByName(request.Name) is not null)
            return (null, $"Hall with name '{request.Name}' already exists.");

        var hall = new Hall { Name = request.Name, Capacity = request.Capacity };

        if (request.Rows.HasValue && request.SeatsPerRow.HasValue)
            hall.GenerateSeats(request.Rows.Value, request.SeatsPerRow.Value);

        _uow.Halls.Add(hall);
        _uow.SaveChanges();

        return (new HallDto
        {
            Id = hall.Id,
            Name = hall.Name,
            Capacity = hall.Capacity,
            SeatCount = hall.Seats.Count
        }, null);
    }

    public bool Delete(long id)
    {
        var hall = _uow.Halls.GetById(id);
        if (hall is null) return false;

        _uow.Halls.Remove(hall);
        _uow.SaveChanges();
        return true;
    }
}