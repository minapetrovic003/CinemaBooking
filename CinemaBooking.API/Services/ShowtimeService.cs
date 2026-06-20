using CinemaBooking.Application.DTOs.Showtimes;
using CinemaBooking.Application.Services;   // <-- izmenjeno iz CinemaBooking.API.Services
using CinemaBooking.Domain;
using CinemaBooking.Domain.Repositories;

namespace CinemaBooking.API.Services;

public class ShowtimeService : IShowtimeService
{
    private readonly IUnitOfWork _uow;

    public ShowtimeService(IUnitOfWork uow) => _uow = uow;

    public IEnumerable<ShowtimeDto> GetAll(string? movieTitle, DateTime? fromDate) =>
        _uow.Showtimes.Search(movieTitle, fromDate).Select(MapToDto);

    public ShowtimeDto? GetById(long id)
    {
        var s = _uow.Showtimes.GetByIdWithDetails(id);
        return s is null ? null : MapToDto(s);
    }

    public (ShowtimeDto? dto, string? errorMessage, int statusCode) Create(CreateShowtimeRequest request)
    {
        var movie = _uow.Movies.GetByTitle(request.MovieTitle);
        if (movie is null)
            return (null, $"Movie '{request.MovieTitle}' not found.", 404);

        var hall = _uow.Halls.GetByNameWithShowtimes(request.HallName);
        if (hall is null)
            return (null, $"Hall '{request.HallName}' not found.", 404);

        var endTime = request.StartTime.AddMinutes(movie.DurationMinutes);

        if (!hall.IsAvailable(request.StartTime, endTime))
            return (null, "Hall is not available at this time.", 409);

        var showtime = new Showtime
        {
            StartTime = request.StartTime,
            EndTime = endTime,
            Price = request.Price,
            MovieId = movie.Id,
            HallId = hall.Id
        };

        _uow.Showtimes.Add(showtime);
        _uow.SaveChanges();

        return (new ShowtimeDto
        {
            Id = showtime.Id,
            StartTime = showtime.StartTime,
            EndTime = showtime.EndTime,
            Price = showtime.Price,
            MovieTitle = movie.Title,
            MovieGenre = movie.Genre,
            MovieDurationMinutes = movie.DurationMinutes,
            HallName = hall.Name,
            HallCapacity = hall.Capacity,
            BookingCount = 0,
            AvailableSeats = hall.Capacity
        }, null, 201);
    }

    public bool Delete(long id)
    {
        var showtime = _uow.Showtimes.GetById(id);
        if (showtime is null) return false;

        _uow.Showtimes.Remove(showtime);
        _uow.SaveChanges();
        return true;
    }

    private static ShowtimeDto MapToDto(Showtime s)
    {
        var confirmed = s.Bookings?
            .Where(b => b.Status != BookingStatus.Canceled).ToList()
            ?? new List<Booking>();

        var bookedSeats = confirmed.SelectMany(b => b.BookingSeats).Count();

        return new ShowtimeDto
        {
            Id = s.Id,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            Price = s.Price,
            MovieTitle = s.Movie?.Title ?? string.Empty,
            MovieGenre = s.Movie?.Genre ?? string.Empty,
            MovieDurationMinutes = s.Movie?.DurationMinutes ?? 0,
            HallName = s.Hall?.Name ?? string.Empty,
            HallCapacity = s.Hall?.Capacity ?? 0,
            BookingCount = confirmed.Count,
            AvailableSeats = (s.Hall?.Capacity ?? 0) - bookedSeats
        };
    }
}