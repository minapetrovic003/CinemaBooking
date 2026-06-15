namespace CinemaBooking.Domain;

public class Showtime
{
    public long Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal Price { get; set; }

    public long MovieId { get; set; }
    public long HallId { get; set; }

    public Movie Movie { get; set; } = null!;
    public Hall Hall { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public DateTime CalculateEndTime(int durationMinutes)
        => StartTime.AddMinutes(durationMinutes);

    public bool IsBookingAvailable()
        => StartTime > DateTime.UtcNow && Bookings.Count < Hall.Capacity;
}