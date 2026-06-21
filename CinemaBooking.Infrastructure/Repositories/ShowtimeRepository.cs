using CinemaBooking.Domain.Models;
using CinemaBooking.Application.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Infrastructure.Repositories;

public class ShowtimeRepository : Repository<Showtime>, IShowtimeRepository
{
    public ShowtimeRepository(CinemaBookingContext context) : base(context) { }

    public IEnumerable<Showtime> Search(string? movieTitle, DateTime? fromDate)
    {
        var query = DbSet
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .Include(s => s.Bookings)
                .ThenInclude(b => b.BookingSeats)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(movieTitle))
            query = query.Where(s => s.Movie.Title.Contains(movieTitle));

        if (fromDate.HasValue)
            query = query.Where(s => s.StartTime >= fromDate.Value);

        return query.OrderBy(s => s.StartTime).ToList();
    }

    // ✅ FIX #2: Dodan .ThenInclude(h => h.Seats)
    // Bez ovoga SeatLockService.GetAvailability() uvijek vraća praznu listu sedišta
    // jer Hall.Seats nije učitan iz baze → frontend prikazuje "Could not load seat map"
    public Showtime? GetByIdWithDetails(long id) =>
        DbSet
            .Include(s => s.Movie)
            .Include(s => s.Hall)
                .ThenInclude(h => h.Seats)  // ✅ OVO NEDOSTAJE u originalnom kodu!
            .Include(s => s.Bookings)
                .ThenInclude(b => b.BookingSeats)
            .FirstOrDefault(s => s.Id == id);

    public Showtime? GetByMovieTitleHallAndStartTime(
        string movieTitle, string hallName, DateTime startTime) =>
        DbSet
            .Include(s => s.Movie)
            .Include(s => s.Hall)
                .ThenInclude(h => h.Seats)
            .Include(s => s.Bookings)
            .FirstOrDefault(s =>
                s.Movie.Title == movieTitle &&
                s.Hall.Name == hallName &&
                s.StartTime == startTime);
}