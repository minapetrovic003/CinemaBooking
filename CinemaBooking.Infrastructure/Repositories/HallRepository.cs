using CinemaBooking.Domain;
using CinemaBooking.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Infrastructure.Repositories;

public class HallRepository : Repository<Hall>, IHallRepository
{
    public HallRepository(CinemaBookingContext context) : base(context) { }

    public Hall? GetByIdWithSeats(long id) =>
        DbSet.Include(h => h.Seats)
             .FirstOrDefault(h => h.Id == id);

    public Hall? GetByName(string name) =>
        DbSet.FirstOrDefault(h => h.Name == name);

    public Hall? GetByNameWithShowtimes(string name) =>
        DbSet.Include(h => h.Showtimes)
             .FirstOrDefault(h => h.Name == name);
}