namespace CinemaBooking.Domain.Repositories;

public interface IShowtimeRepository : IRepository<Showtime>
{
    IEnumerable<Showtime> Search(string? movieTitle, DateTime? fromDate);
    Showtime? GetByIdWithDetails(long id);
    Showtime? GetByMovieTitleHallAndStartTime(string movieTitle, string hallName, DateTime startTime);
}