using CinemaBooking.Domain.Models;

namespace CinemaBooking.Application.Repositories;

public interface IUnitOfWork : IDisposable
{
    IMovieRepository Movies { get; }
    IHallRepository Halls { get; }
    IShowtimeRepository Showtimes { get; }
    IBookingRepository Bookings { get; }
    IPaymentRepository Payments { get; }
    IRepository<Seat> Seats { get; }
    IRepository<BookingSeat> BookingSeats { get; }
    ISeatLockRepository SeatLocks { get; }

    int SaveChanges();
    Task<int> SaveChangesAsync();
}