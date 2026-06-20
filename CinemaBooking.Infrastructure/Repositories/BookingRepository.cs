using CinemaBooking.Domain.Models;
using CinemaBooking.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Infrastructure.Repositories;

public class BookingRepository : Repository<Booking>, IBookingRepository
{
    public BookingRepository(CinemaBookingContext context) : base(context) { }

    public IEnumerable<Booking> Search(
        string? userEmail, string? status, DateTime? fromDate, DateTime? toDate)
    {
        var query = DbSet
            .Include(b => b.Showtime)
                .ThenInclude(s => s.Movie)
            .Include(b => b.Showtime)
                .ThenInclude(s => s.Hall)
            .Include(b => b.BookingSeats)
                .ThenInclude(bs => bs.Seat)
            .AsQueryable();

        // Filter po emailu: tražimo UserId iz AspNetUsers tabele
        if (!string.IsNullOrWhiteSpace(userEmail))
        {
            var userId = Context.Users
                .Where(u => u.Email == userEmail)
                .Select(u => u.Id)
                .FirstOrDefault();

            if (userId is not null)
                query = query.Where(b => b.UserId == userId);
            else
                return Enumerable.Empty<Booking>(); // email ne postoji → prazna lista
        }

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<BookingStatus>(status, true, out var bookingStatus))
            query = query.Where(b => b.Status == bookingStatus);

        if (fromDate.HasValue)
            query = query.Where(b => b.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(b => b.CreatedAt <= toDate.Value);

        return query.OrderByDescending(b => b.CreatedAt).ToList();
    }

    public Booking? GetByIdWithDetails(long id) =>
        DbSet
            .Include(b => b.Showtime)
                .ThenInclude(s => s.Movie)
            .Include(b => b.Showtime)
                .ThenInclude(s => s.Hall)
            .Include(b => b.BookingSeats)
                .ThenInclude(bs => bs.Seat)
            .FirstOrDefault(b => b.Id == id);

    public IEnumerable<long> GetBookedSeatIds(long showtimeId) =>
        Context.Set<BookingSeat>()
            .Include(bs => bs.Booking)
            .Where(bs =>
                bs.Booking.ShowtimeId == showtimeId &&
                bs.Booking.Status != BookingStatus.Canceled)
            .Select(bs => bs.SeatId)
            .ToList();
}