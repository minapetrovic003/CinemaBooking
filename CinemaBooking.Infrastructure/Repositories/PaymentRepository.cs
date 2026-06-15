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
            .FirstOrDefault(p => p.Id == id);
}