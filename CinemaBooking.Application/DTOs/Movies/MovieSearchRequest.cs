namespace CinemaBooking.Application.DTOs.Movies;

public class MovieSearchRequest
{
    public string? Title { get; set; }
    public string? Genre { get; set; }
    public decimal? MinRating { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}