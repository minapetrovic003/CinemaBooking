namespace CinemaBooking.Domain.Models;

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

    // OPTIMISTIC CONCURRENCY 
    // SQL Server automatski ažurira ovaj bajt niz pri svakom UPDATE-u.
    // EF Core koristi ovu vrednost u WHERE klauzuli: ako se vrednost promenila
    // između čitanja i upisa, SaveChanges() baca DbUpdateConcurrencyException.
    // Na taj način two simultaneous bookings of the same seat are guaranteed
    // to result in exactly one success and one concurrency conflict.
    [System.ComponentModel.DataAnnotations.Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
   
    public DateTime CalculateEndTime(int durationMinutes)
        => StartTime.AddMinutes(durationMinutes);

    public bool IsBookingAvailable()
        => StartTime > DateTime.UtcNow && Bookings.Count < Hall.Capacity;
}