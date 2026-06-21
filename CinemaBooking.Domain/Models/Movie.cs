namespace CinemaBooking.Domain.Models;

public class Movie
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public decimal Rating { get; set; }

    
    public ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();

    public void UpdateDetails(string title, string description)
    {
        Title = title;
        Description = description;
    }
}