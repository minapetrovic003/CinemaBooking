namespace CinemaBooking.Domain.Repositories;

public interface IBookingRepository : IRepository<Booking>
{
    IEnumerable<Booking> Search(string? userEmail, string? status, DateTime? fromDate, DateTime? toDate);
    Booking? GetByIdWithDetails(long id);
    IEnumerable<long> GetBookedSeatIds(long showtimeId);
}