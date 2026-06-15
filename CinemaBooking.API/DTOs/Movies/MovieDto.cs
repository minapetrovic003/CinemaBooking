namespace CinemaBooking.API.DTOs.Movies;

public class MovieDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public decimal Rating { get; set; }
    public int ShowtimeCount { get; set; }
}