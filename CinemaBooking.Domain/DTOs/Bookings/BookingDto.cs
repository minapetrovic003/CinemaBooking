namespace CinemaBooking.Domain.DTOs.Bookings;

public class BookingDto
{
    public long Id { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public string UserFullName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;

    public string MovieTitle { get; set; } = string.Empty;
    public string MovieGenre { get; set; } = string.Empty;
    public int MovieDurationMinutes { get; set; }
    public string HallName { get; set; } = string.Empty;
    public DateTime ShowtimeStart { get; set; }

    public List<BookingSeatDto> Seats { get; set; } = new();
}

public class BookingSeatDto
{
    public string SeatLabel { get; set; } = string.Empty;  
    public string SeatType { get; set; } = string.Empty;   
    public decimal Price { get; set; }
}