using CinemaBooking.Domain;
using CinemaBooking.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Infrastructure.Repositories;

public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(CinemaBookingContext context) : base(context) { }

    public Payment? GetByIdWithDetails(long id) =>
        DbSet
            .Include(p => p.Booking)
                .ThenInclude(b => b.Showtime)
                    .ThenInclude(s => s.Movie)
            .Include(p => p.Booking)
                .ThenInclude(b => b.Showtime)
                    .ThenInclude(s => s.Hall)   // NOVO — potreban za email sadržaj
            .Include(p => p.Booking)
                .ThenInclude(b => b.BookingSeats)   // NOVO — potreban za detalje
                    .ThenInclude(bs => bs.Seat)
            .FirstOrDefault(p => p.Id == id);
}