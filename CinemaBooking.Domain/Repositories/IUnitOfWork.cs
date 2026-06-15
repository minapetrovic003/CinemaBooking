namespace CinemaBooking.Domain.Repositories;

public interface IUnitOfWork : IDisposable
{
    IMovieRepository Movies { get; }
    IHallRepository Halls { get; }
    IShowtimeRepository Showtimes { get; }
    IBookingRepository Bookings { get; }
    IPaymentRepository Payments { get; }
    IRepository<Seat> Seats { get; }
    IRepository<BookingSeat> BookingSeats { get; }

    int SaveChanges();
    Task<int> SaveChangesAsync();
}