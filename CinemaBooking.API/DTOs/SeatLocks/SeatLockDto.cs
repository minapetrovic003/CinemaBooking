namespace CinemaBooking.API.DTOs.SeatLocks;

public class SeatLockDto
{
    public List<string> LockedSeats { get; set; } = new();
    public DateTime ExpiresAt { get; set; }

    //Sekunde do isteka lock-a — korisno za countdown timer na frontendu
    public int ExpiresInSeconds => Math.Max(0, (int)(ExpiresAt - DateTime.UtcNow).TotalSeconds);

    public string Message { get; set; } = string.Empty;
}
