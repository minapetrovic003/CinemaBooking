namespace CinemaBooking.Domain.DTOs.Movies;

public class UpdateMovieRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public decimal Rating { get; set; }
}