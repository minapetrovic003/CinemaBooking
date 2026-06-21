using CinemaBooking.Domain.Models;

namespace CinemaBooking.Application.Repositories;

public interface IHallRepository : IRepository<Hall>
{
    Hall? GetByIdWithSeats(long id);
    Hall? GetByName(string name);
    Hall? GetByNameWithShowtimes(string name);
}