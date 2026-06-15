using CinemaBooking.API.DTOs.Halls;

namespace CinemaBooking.API.Services;

public interface IHallService
{
    IEnumerable<HallDto> GetAll();
    HallDto? GetById(long id);
    (HallDto? dto, string? conflictMessage) Create(CreateHallRequest request);
    bool Delete(long id);
}