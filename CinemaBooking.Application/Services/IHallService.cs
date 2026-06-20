using CinemaBooking.Domain.DTOs.Halls;

namespace CinemaBooking.Application.Services;

public interface IHallService
{
    IEnumerable<HallDto> GetAll();
    HallDto? GetById(long id);
    (HallDto? dto, string? conflictMessage) Create(CreateHallRequest request);
    bool Delete(long id);
}