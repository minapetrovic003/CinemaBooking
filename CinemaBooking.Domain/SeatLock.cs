namespace CinemaBooking.Domain;

public class SeatLock
{
    public long Id { get; set; }

    public long SeatId { get; set; }
    public long ShowtimeId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public DateTime LockedAt { get; set; } = DateTime.UtcNow;

    
    public DateTime ExpiresAt { get; set; }

   
    public Seat Seat { get; set; } = null!;
    public Showtime Showtime { get; set; } = null!;

    public bool IsActive() => ExpiresAt > DateTime.UtcNow;

    public bool OwnedBy(string userId) => UserId == userId;

    public static SeatLock Create(long seatId, long showtimeId, string userId,
        int lockMinutes = 10) => new()
        {
            SeatId = seatId,
            ShowtimeId = showtimeId,
            UserId = userId,
            LockedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(lockMinutes)
        };
}
