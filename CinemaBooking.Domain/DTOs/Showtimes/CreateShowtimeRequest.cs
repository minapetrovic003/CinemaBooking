namespace CinemaBooking.Domain.DTOs.Showtimes;

public class CreateShowtimeRequest
{
    public string MovieTitle { get; set; } = string.Empty; 
    public string HallName { get; set; } = string.Empty;   
    public DateTime StartTime { get; set; }
    public decimal Price { get; set; }
}