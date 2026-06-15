namespace CinemaBooking.API.DTOs.Bookings;

public class CreateBookingRequest
{
    public string UserEmail { get; set; } = string.Empty;
    public string MovieTitle { get; set; } = string.Empty;
    public string HallName { get; set; } = string.Empty;
    public DateTime ShowtimeStartTime { get; set; }
    public List<string> Seats { get; set; } = new();  
}