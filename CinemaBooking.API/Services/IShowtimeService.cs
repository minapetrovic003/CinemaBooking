using CinemaBooking.API.DTOs.Showtimes;

namespace CinemaBooking.API.Services;

public interface IShowtimeService
{
    IEnumerable<ShowtimeDto> GetAll(string? movieTitle, DateTime? fromDate);
    ShowtimeDto? GetById(long id);
    (ShowtimeDto? dto, string? errorMessage, int statusCode) Create(CreateShowtimeRequest request);
    bool Delete(long id);
}