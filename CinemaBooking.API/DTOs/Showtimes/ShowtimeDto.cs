namespace CinemaBooking.API.DTOs.Showtimes;

public class ShowtimeDto
{
    public long Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal Price { get; set; }

    public string MovieTitle { get; set; } = string.Empty;
    public string MovieGenre { get; set; } = string.Empty;
    public int MovieDurationMinutes { get; set; }
    public string HallName { get; set; } = string.Empty;
    public int HallCapacity { get; set; }
    public int BookingCount { get; set; }
    public int AvailableSeats { get; set; }
}