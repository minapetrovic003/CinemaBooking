namespace CinemaBooking.Application.DTOs.SeatLocks;

public class SeatLockDto
{
    public List<string> LockedSeats { get; set; } = new();
    public DateTime ExpiresAt { get; set; }

    // Seconds until lock expiry — useful for countdown timer on frontend
    public int ExpiresInSeconds => Math.Max(0, (int)(ExpiresAt - DateTime.UtcNow).TotalSeconds);

    public string Message { get; set; } = string.Empty;
}
