using CinemaBooking.Domain;
using CinemaBooking.Domain.Repositories;
using CinemaBooking.Infrastructure.Repositories;

namespace CinemaBooking.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly CinemaBookingContext _context;

    private IMovieRepository? _movies;
    private IHallRepository? _halls;
    private IShowtimeRepository? _showtimes;
    private IBookingRepository? _bookings;
    private IPaymentRepository? _payments;
    private IRepository<Seat>? _seats;
    private IRepository<BookingSeat>? _bookingSeats;

    public UnitOfWork(CinemaBookingContext context)
    {
        _context = context;
    }

    public IMovieRepository Movies =>
        _movies ??= new MovieRepository(_context);

    public IHallRepository Halls =>
        _halls ??= new HallRepository(_context);

    public IShowtimeRepository Showtimes =>
        _showtimes ??= new ShowtimeRepository(_context);

    public IBookingRepository Bookings =>
        _bookings ??= new BookingRepository(_context);

    public IPaymentRepository Payments =>
        _payments ??= new PaymentRepository(_context);

    public IRepository<Seat> Seats =>
        _seats ??= new Repository<Seat>(_context);

    public IRepository<BookingSeat> BookingSeats =>
        _bookingSeats ??= new Repository<BookingSeat>(_context);

    public int SaveChanges() => _context.SaveChanges();
    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
    public void Dispose() => _context.Dispose();
}